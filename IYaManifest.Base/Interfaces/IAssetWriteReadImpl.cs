using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Interfaces
{
    /// <summary>
    /// 资源读写实现类
    /// </summary>
    public interface IAssetWriteReadImpl
    {
        /// <summary>
        /// 从流载入资源
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        IOperationResult<IAsset> LoadFrom(Stream stream);
        /// <summary>
        /// 从 byte[] 载入资源
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        IOperationResult<IAsset> LoadFrom(byte[] data);
        /// <summary>
        /// 将资源序列化后, 写入流
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        IOperationResult WriteTo(IAsset asset, Stream stream);
        /// <summary>
        /// 将资源序列化为 byte[] 
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        IOperationResult<byte[]> Serialization(IAsset asset);
    }
    /// <summary>
    /// 特定资源的读写实现类
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public interface IAssetWriteReadImpl<TAsset> : IAssetWriteReadImpl
        where TAsset : IAsset 
    {
        /// <summary>
        /// 从流载入资源
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        new IOperationResult<TAsset> LoadFrom(Stream stream);

        /// <summary>
        /// 从 byte[] 载入资源
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        new IOperationResult<TAsset> LoadFrom(byte[] data);

        /// <summary>
        /// 将资源序列化后, 写入流
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        IOperationResult WriteTo(TAsset asset, Stream stream);

        /// <summary>
        /// 将资源序列化为 byte[] 
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        IOperationResult<byte[]> Serialization(TAsset asset);
    }

    /// <summary>
    /// 资源读写实现的通用基类, 只能处理 <see cref="TAsset"/>
    /// <para>传入 <see cref="IAsset"/> 类型时, 会检查是否 <see cref="TAsset"/>, 如果不是, 则抛出异常</para>
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public abstract class AssetWriteReadImplBase<TAsset> : IAssetWriteReadImpl<TAsset>
        where TAsset : IAsset
    {

        public abstract IOperationResult<TAsset> LoadFrom(Stream stream);

        public abstract IOperationResult<TAsset> LoadFrom(byte[] data);

        public abstract IOperationResult<byte[]> Serialization(TAsset asset);

        public abstract IOperationResult WriteTo(TAsset asset, Stream stream);

        public IOperationResult<byte[]> Serialization(IAsset asset)
        {
            _checkAssetType(asset);
            return Serialization((TAsset)asset);
        }


        public IOperationResult WriteTo(IAsset asset, Stream stream)
        {
            _checkAssetType(asset);
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
        private void _checkAssetType(IAsset asset)
        {
            ArgumentNullException.ThrowIfNull(asset, nameof(asset));
            if (asset.GetType() != typeof(TAsset))
            {
                throw new ArgumentException($"读写实现仅支持类型与泛型相同的资源 ({typeof(TAsset)})");
            }
        }
        #endregion
    }

    /// <summary>
    /// 资源读写实现的通用基类的扩展版本①, 只能处理 <see cref="TAsset"/>
    /// <para><see cref="AssetWriteReadImplBase.LoadFrom(byte[])"/> 与 <see cref="AssetWriteReadImplBase.Serialization(TAsset)"/> 分别以内存流的形式, </para>
    /// <para>调用 <see cref="AssetWriteReadImplBase.LoadFrom(Stream)"/> 与 <see cref="AssetWriteReadImplBase.WriteTo(TAsset, Stream)"/> 实现</para>
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public abstract class AssetWriteReadImplBaseEx1<TAsset> : AssetWriteReadImplBase<TAsset>
        where TAsset : IAsset
    {
        public override IOperationResult<TAsset> LoadFrom(byte[] data)
        {
            using MemoryStream ms = new MemoryStream(data);
            return LoadFrom(ms);
        }

        public override IOperationResult<byte[]> Serialization(TAsset asset)
        {
            using MemoryStream ms = new();
            var result = WriteTo(asset, ms);
            byte[] bs = ms.ToArray();
            return new OperationResult<byte[]>()
            {
                Data = bs,
                FailureReason = result.FailureReason,
                IsSuccess = result.IsSuccess,
                SuccessInfo = result.SuccessInfo,
            };
        }
    }

    public static class IAssetWriteReadImplExtensions
    {
        /// <summary>
        /// 从 base64 编码的字符串载入资源
        /// </summary>
        /// <param name="impl"></param>
        /// <param name="base64"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IOperationResult<IAsset> LoadFrom(this IAssetWriteReadImpl impl, string base64)
        {
            byte[] bs = Convert.FromBase64String(base64);
            return impl.LoadFrom(bs);
        }
        /// <summary>
        /// 从 base64 编码的字符串载入资源
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="impl"></param>
        /// <param name="base64"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  
        public static IOperationResult<TAsset> LoadFrom<TAsset>(this IAssetWriteReadImpl<TAsset> impl, string base64)
            where TAsset : IAsset
        {
            byte[] bs = Convert.FromBase64String(base64);
            return impl.LoadFrom(bs);
        }
    }
}
