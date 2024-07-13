using Common_Util;
using Common_Util.Data.Exceptions;
using Common_Util.Data.Struct;
using Common_Util.Data.Structure.Value;
using Common_Util.IO;
using IYaManifest.Core.Base;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Common_Util.Module.LayerComponentBaseLong;

namespace IYaManifest.Core.V1
{
    /// <summary>
    /// 自定义资源加载的处理器
    /// </summary>
    public abstract class CustomAssetLoadHandler
    {
        public abstract void BeforeReadItems(ManifestHead head);
        public abstract void AfterReadItems(ManifestItem[] items);
        public abstract void ManifestReadFailure(IOperationResult result);

        /// <summary>
        /// 处理清单项
        /// </summary>
        /// <param name="item">从文件中读取得到的清单项</param>
        /// <param name="stream">对应的数据流</param>
        /// <param name="dataLength">数据流的总长度, 如果为0, 则有可能是文件不存在等情况</param>
        /// <param name="writeReader">当前需要使用的资源读写器</param>
        /// <returns>返回值将被赋值到 <see cref="ManifestItem.AssetReference"/></returns>
        public abstract IAsset Handle(ManifestItem item, Stream stream, long dataLength, AssetWriteReader writeReader);
    }

    /// <summary>
    /// 默认的将数据直接转换为对象的处理方法
    /// </summary>
    public class DefaultToObjectLoadHanlder : CustomAssetLoadHandler
    {
        public static DefaultToObjectLoadHanlder Instance => TypeHelper.Singleton<DefaultToObjectLoadHanlder>(); 

        public override void AfterReadItems(ManifestItem[] items)
        {
        }

        public override void BeforeReadItems(ManifestHead head)
        {
        }
        public override void ManifestReadFailure(IOperationResult result)
        {
        }

        public override IAsset Handle(ManifestItem item, Stream stream, long dataLength, AssetWriteReader writeReader)
        {
            var result = writeReader.LoadFrom(item.AssetType, stream);
            if (result.IsFailure)
            {
                throw new OperationFailureException(result);
            }
            if (result.Data == null)
            {
                throw new OperationFailureException((OperationResult)$"资源 ({item.AssetId}) 加载操作成功, 但返回对象为 null 值! ");
            }
            return result.Data;
        }

    }

    /// <summary>
    /// 默认的外部资源文件加载为懒资源对象的处理方法
    /// </summary>
    public class DefaultOutsideLazyFileAssetLoadHandler : CustomAssetLoadHandler
    {
        public static DefaultOutsideLazyFileAssetLoadHandler Instance => TypeHelper.Singleton<DefaultOutsideLazyFileAssetLoadHandler>();

        private ManifestHead? head;
        public override void BeforeReadItems(ManifestHead head)
        {
            this.head = head;
        }
        public override void AfterReadItems(ManifestItem[] items)
        {
        }
        public override void ManifestReadFailure(IOperationResult result)
        {
        }

        /// <summary>
        /// 对资源做一些处理操作, 在 <see cref="Handle"/> 方法即将返回前调用
        /// </summary>
        public Func<LazyFileAsset, LazyFileAsset>? HandleLazyAsset { get; set; }


        public override IAsset Handle(ManifestItem item, Stream stream, long dataLength, AssetWriteReader writeReader)
        {
            ArgumentNullException.ThrowIfNull(this.head);
            ArgumentException.ThrowIfNullOrWhiteSpace(head.AbsolutePath);
            ArgumentException.ThrowIfNullOrWhiteSpace(item.OutsidePath);

            if (!writeReader.CurrentMapping.TryGet(item.AssetType, out var config))
            {
                throw new OperationFailureException((OperationResult)$"未取得资源类型 {item.AssetType} 对应的映射配置");
            }
            else if (config == null)
            {
                throw new OperationFailureException((OperationResult)$"获取资源类型 {item.AssetType} 对应的映射配置取得 null! ");
            }
            
            string path = PathHelper.GetAbsolutePath(head.AbsolutePath, item.OutsidePath);  // 外部资源的绝对路径
            LazyFileAsset output = new LazyFileAsset()
            {
                TargetAssetClass = config.AssetClass,
                AssetId = item.AssetId,
                AssetType = item.AssetType,
                FileName = path,
                Start = 0,
                Length = (uint)dataLength,
                ReadImpl = (IAssetWriteReadImpl)config!.InstanceWriteReadImpl(),
            };

            if (HandleLazyAsset != null)
            {
                HandleLazyAsset(output);
            }

            return output;
        }
    }

    /// <summary>
    /// 将资源缓存到某个文件的处理方法
    /// </summary>
    public class CacheToStreamLoadHandler : CustomAssetLoadHandler
    {
        public CacheToStreamLoadHandler(string fileName) 
        {
            this.fileName = fileName;
        }

        private Stream? stream;
        private long current;
        private readonly string fileName;

        public override void AfterReadItems(ManifestItem[] items)
        {
            stream?.Flush();
            stream?.Dispose();
        }

        public override void BeforeReadItems(ManifestHead head)
        {
            stream = File.OpenWrite(fileName);
            current = 0;
        }
        public override void ManifestReadFailure(IOperationResult result)
        {
            stream?.Dispose();
        }

        /// <summary>
        /// 对资源做一些处理操作, 在 <see cref="Handle"/> 方法即将返回前调用
        /// </summary>
        public Func<LazyFileAsset, LazyFileAsset>? HandleLazyAsset { get; set; }


        public override IAsset Handle(ManifestItem item, Stream stream, long dataLength, AssetWriteReader writeReader)
        {
            ArgumentNullException.ThrowIfNull(this.stream);

            if (!writeReader.CurrentMapping.TryGet(item.AssetType, out var config))
            {
                throw new OperationFailureException((OperationResult)$"未取得资源类型 {item.AssetType} 对应的映射配置");
            }
            else if (config == null)
            {
                throw new OperationFailureException((OperationResult)$"获取资源类型 {item.AssetType} 对应的映射配置取得 null! ");
            }


            LazyFileAsset output = new LazyFileAsset(){
                TargetAssetClass = config.AssetClass,
                AssetId = item.AssetId,
                AssetType = item.AssetType,
                FileName = fileName,
                Start = (uint)current,
                Length = (uint)dataLength,
                ReadImpl = (IAssetWriteReadImpl)config!.InstanceWriteReadImpl(),
            };
            byte[] buffer = new byte[4096];
            int count;
            stream.Seek(0, SeekOrigin.Begin);
            while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                this.stream.Write(buffer, 0, count);
            }
            current += dataLength;

            if (HandleLazyAsset != null)
            {
                HandleLazyAsset(output);
            }

            return output;
        }
    }
}
