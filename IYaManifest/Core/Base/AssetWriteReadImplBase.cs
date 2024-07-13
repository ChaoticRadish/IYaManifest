using Common_Util.Data.Struct;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Core.Base
{
    public abstract class AssetWriteReadImplBase<TAsset> : IAssetWriteReadImpl<TAsset>
        where TAsset : IAsset
    {

        public abstract IOperationResult<TAsset> LoadFrom(Stream stream);

        public abstract IOperationResult<TAsset> LoadFrom(byte[] data);

        public abstract IOperationResult<byte[]> Serialization(TAsset asset);

        public abstract IOperationResult WriteTo(TAsset asset, Stream stream);

        public IOperationResult<byte[]> Serialization(IAsset asset)
        {
            checkAssetType(asset);
            return Serialization((TAsset)asset);
        }


        public IOperationResult WriteTo(IAsset asset, Stream stream)
        {
            checkAssetType(asset);
            return WriteTo((TAsset)asset, stream);
        }

        IOperationResult<IAsset> IAssetWriteReadImpl.LoadFrom(Stream stream)
        {
            var result = ((IAssetWriteReadImpl<TAsset>)this).LoadFrom(stream);

            return new OperationResult<IAsset>()
            {
                IsSuccess = result.IsSuccess,
                Data = result.Data,
                FailureReason = result.FailureReason,
                SuccessInfo = result.SuccessInfo,
            };
        }

        IOperationResult<IAsset> IAssetWriteReadImpl.LoadFrom(byte[] data)
        {
            var result = ((IAssetWriteReadImpl<TAsset>)this).LoadFrom(data);

            return new OperationResult<IAsset>()
            {
                IsSuccess = result.IsSuccess,
                Data = result.Data,
                FailureReason = result.FailureReason,
                SuccessInfo = result.SuccessInfo,
            };
        }

        #region 检查
        private void checkAssetType(IAsset asset) 
        {
            if (asset.GetType() != typeof(TAsset))
            {
                throw new ArgumentException($"读写实现仅支持类型与泛型相同的资源 ({typeof(TAsset)})");
            }
        }
        #endregion
    }
}
