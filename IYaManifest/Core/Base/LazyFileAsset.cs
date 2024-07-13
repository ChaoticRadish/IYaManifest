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
    /// 数据存储在文件中的懒加载资源, 其 <see cref="Dispose"/> 方法仅调用 <see cref="Clear"/> 方法, 以方便使用
    /// </summary>
    public class LazyFileAsset : LazyFileAsset<IAsset> 
    {
        /// <summary>
        /// 目标资源类
        /// </summary>
        public required Type TargetAssetClass { get; set; }
    }
    /// <summary>
    /// 数据存储在文件中的懒加载资源, 其 <see cref="Dispose"/> 方法仅调用 <see cref="Clear"/> 方法, 以方便使用
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public class LazyFileAsset<TAsset> : IAsset, IDisposable
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

        /// <summary>
        /// 资源, 未加载状态下调用会从文件中读取数据并加载为对象
        /// </summary>
        public TAsset? Asset 
        {
            get
            {
                if (!loaded)
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
                        }
                        loaded = true;
                        
                    }
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


        /// <summary>
        /// 清理当前已加载的资源. 线程不安全, 如果在资源加载过程中调用, 可能会导致错误
        /// </summary>
        public void Clear()
        {
            lock (locker)
            {
                _asset = default;
                if (loaded)
                {
                    OnUnload?.Invoke(this);
                }
                loaded = false;
            }
        }

        /// <summary>
        /// 调用 <see cref="Clear"/>
        /// </summary>
        public void Dispose()
        {
            Clear();
        }

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
        #endregion
    }
}
