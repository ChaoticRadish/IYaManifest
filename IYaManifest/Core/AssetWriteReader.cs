using Common_Util.Data.Struct;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Core
{
    /// <summary>
    /// 资源读写器
    /// </summary>
    public class AssetWriteReader
    {
        #region 全局值
        private readonly static Lazy<AssetWriteReader> lazyDefault = new(() => new AssetWriteReader());
        /// <summary>
        /// 通用缺省值单例
        /// </summary>
        public static AssetWriteReader Default => lazyDefault.Value;
        #endregion

        #region 实现映射
        /// <summary>
        /// 读写实现类型的映射
        /// </summary>
        public MappingConfig? Mapping { get; set; }

        /// <summary>
        /// 如果 <see cref="Mapping"/> == null, 则返回 <see cref="DefaultMapping"/>
        /// </summary>
        public MappingConfig CurrentMapping { get => Mapping ?? DefaultMapping; }
        #endregion

        #region 默认映射
        /// <summary>
        /// 默认的读写实现映射, 实例化时未设置 <see cref="Mapping"/>, 那么将会使用这里配置的映射
        /// </summary>
        public static MappingConfig DefaultMapping { get; private set; } = new();
        #endregion



        #region 操作
        /// <summary>
        /// 使用映射配置的读写实现, 将流数据读取为资源
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IOperationResultEx<IAsset> LoadFrom(string assetType, Stream stream)
        {
            OperationResultEx<IAsset> result;
            try
            {
                if (!CurrentMapping.TryGet(assetType, out var config))
                {
                    return result = $"未取得资源类型 {assetType} 对应的映射配置";
                }

                var wrImpl = (IAssetWriteReadImpl)config!.InstanceWriteReadImpl();
                var loadResult = wrImpl.LoadFrom(stream);
                if (loadResult.IsFailure)
                {
                    return result = "从流读取失败, " + loadResult.FailureReason;
                }

                return result = OperationResultEx<IAsset>.Success(loadResult.Data);

            }
            catch (Exception ex)
            {
                return result = ex;
            }
        }
        /// <summary>
        /// 使用映射配置的读写实现, 将 byte[] 数据读取为资源
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public IOperationResultEx<IAsset> LoadFrom(string assetType, byte[] data)
        {
            OperationResultEx<IAsset> result;
            try
            {
                if (!CurrentMapping.TryGet(assetType, out var config))
                {
                    return result = $"未取得资源类型 {assetType} 对应的映射配置";
                }

                var wrImpl = (IAssetWriteReadImpl)config!.InstanceWriteReadImpl();
                var loadResult = wrImpl.LoadFrom(data);
                if (loadResult.IsFailure)
                {
                    return result = "从 byte[] 读取失败, " + loadResult.FailureReason;
                }

                return result = OperationResultEx<IAsset>.Success(loadResult.Data);

            }
            catch (Exception ex)
            {
                return result = ex;
            }
        }
        /// <summary>
        /// 使用映射配置的读写实现, 将资源写入流
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="asset"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public IOperationResultEx WriteTo(string assetType, IAsset asset, Stream stream)
        {
            OperationResultEx result;
            try
            {
                if (!CurrentMapping.TryGet(assetType, out var config))
                {
                    return result = $"未取得资源类型 {assetType} 对应的映射配置";
                }
                if (asset.GetType() != config!.AssetClass)
                {
                    return result = "传入资源类型与映射类型不匹配! ";
                }

                var wrImpl = (IAssetWriteReadImpl)config!.InstanceWriteReadImpl();
                var writeResult = wrImpl.WriteTo(asset, stream);
                if (writeResult.IsFailure)
                {
                    return result = "将资源写入流失败, " + writeResult.FailureReason;
                }

                return result = true;

            }
            catch (Exception ex)
            {
                return result = ex;
            }
        }
        /// <summary>
        /// 使用映射配置的读写实现, 将资源序列化
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="asset"></param>
        /// <returns></returns>
        public IOperationResultEx<byte[]> Serialization(string assetType, IAsset asset)
        {
            OperationResultEx<byte[]> result;
            try
            {
                if (!CurrentMapping.TryGet(assetType, out var config))
                {
                    return result = $"未取得资源类型 {assetType} 对应的映射配置";
                }
                if (asset.GetType() != config!.AssetClass)
                {
                    return result = "传入资源类型与映射类型不匹配! ";
                }

                var wrImpl = (IAssetWriteReadImpl)config!.InstanceWriteReadImpl();
                var sResult = wrImpl.Serialization(asset);
                if (sResult.IsFailure)
                {
                    return result = "将资源序列化失败, " + sResult.FailureReason;
                }

                return result = sResult.Data!;

            }
            catch (Exception ex)
            {
                return result = ex;
            }
        }
        #endregion


    }
}
