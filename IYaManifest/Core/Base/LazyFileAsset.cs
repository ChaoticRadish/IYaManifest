using Common_Util.Data.Exceptions;
using Common_Util.Data.Struct;
using Common_Util.Streams;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Core.Base
{
    /// <summary>
    /// 接口: 数据存储在文件中的懒加载资源
    /// </summary>
    public interface ILazyFileAsset : IAsset, ILazyAsset
    {
        public string FileName { get; set; }
        public uint Start { get; set; }
        public uint Length { get; set; }
    }

    /// <summary>
    /// 数据存储在文件中的懒加载资源, 其 <see cref="Dispose"/> 方法仅调用 <see cref="Clear"/> 方法, 以方便使用
    /// </summary>
    public class LazyFileAsset : LazyFileAsset<IAsset>, ILazyFileAsset
    {
        /// <summary>
        /// 目标资源类
        /// </summary>
        public required Type TargetAssetClass { get; set; }

        public override Type ExpectReferenceType => TargetAssetClass;
    }
    /// <summary>
    /// 数据存储在文件中的懒加载资源, 其 <see cref="Dispose"/> 方法仅调用 <see cref="Unload"/> 方法, 以方便使用
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public class LazyFileAsset<TAsset> : ILazyAsset<TAsset>, IDisposable, ILazyFileAsset
        where TAsset : IAsset
    {
        
        public string AssetId { get; set; } = string.Empty;

        public string AssetType { get; set; } = string.Empty;

        /// <summary>
        /// 数据存储文件
        /// </summary>
        public required string FileName { get; set; } 
        /// <summary>
        /// 数据在文件中的起点
        /// </summary>
        public required uint Start { get; set; }
        /// <summary>
        /// 数据在文件中的占用长度
        /// </summary>
        public required uint Length { get; set; }
        /// <summary>
        /// 读取实现
        /// </summary>
        public required IAssetWriteReadImpl ReadImpl { get; set; }

        IAsset? ILazyAsset.Asset { get => Asset; }

        /// <summary>
        /// 未加载状态下, 获取属性 <see cref="Asset"/> 时, 是否自动调用加载
        /// </summary>
        public bool AutoLoadAsset { get; set; } = false;
        /// <summary>
        /// 资源, 未加载状态下调用会从文件中读取数据并加载为对象
        /// </summary>
        public TAsset? Asset 
        {
            get
            {
                if (AutoLoadAsset && !loaded)
                {
                    Load();
                }
                return _asset;
            }
        }
        /// <summary>
        /// 取得资源, 不触发加载, 如果资源未被加载, 有可能返回 null
        /// </summary>
        /// <returns></returns>
        public TAsset? GetAssetAndNotLoad() => _asset;
        private readonly object locker = new();
        private bool loaded = false;
        private TAsset? _asset;

        /// <summary>
        /// 资源是否已加载
        /// </summary>
        public bool Loaded => loaded;

        public virtual Type ExpectReferenceType => typeof(TAsset);


        /// <summary>
        /// 调用 <see cref="Unload"/>
        /// </summary>
        public void Dispose()
        {
            Unload();
        }

        #region 加卸载操作
        public void Load()
        {
            lock (locker)
            {
                if (!loaded)
                {
                    using var fs = File.OpenRead(FileName);
                    using OffsetWrapperStream ows = new OffsetWrapperStream(fs, Start, Length);
                    var result = ReadImpl.LoadFrom(ows);
                    if (result.IsSuccess)
                    {
                        if (result.Data == null)
                        {
                            throw new OperationFailureException((OperationResult)$"加载方法执行成功, 但是结果中的数据值为 null");
                        }
                        _asset = (TAsset)result.Data;
                        _asset.AssetId = AssetId;
                        OnLoad?.Invoke(this, _asset);
                    }
                    else
                    {
                        throw new OperationFailureException(result);
                    }

                    loaded = true;
                    LoadedStateChanged?.Invoke(this, EventArgs.Empty);
                }

            }
        }

        /// <summary>
        /// 清理当前已加载的资源. 线程不安全, 如果在资源加载过程中调用, 可能会导致错误
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Unload()
        {
            lock (locker)
            {
                _asset = default;
                if (loaded)
                {
                    OnUnload?.Invoke(this);
                    loaded = false;
                    LoadedStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        #region 事件
        /// <summary>
        /// 资源加载后调用, 同步运行.
        /// <para>在方法内不能调用卸载资源的方法, 否则将产生互锁</para>
        /// </summary>
        public event Action<LazyFileAsset<TAsset>, TAsset>? OnLoad;
        /// <summary>
        /// 资源卸载后调用, 同步运行.
        /// <para>在方法内不能调用加载资源的方法, 否则将产生互锁</para>
        /// </summary>
        public event Action<LazyFileAsset<TAsset>>? OnUnload;

        public event EventHandler? LoadedStateChanged;
        #endregion
    }
}
