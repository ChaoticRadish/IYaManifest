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
