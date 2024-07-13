using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace IYaManifest.Interfaces
{
    /// <summary>
    /// 清单文件的读取器
    /// </summary>
    public interface IManifestFileReader : IDisposable
    {
        /// <summary>
        /// 将文件读取为目标类型的清单
        /// </summary>
        /// <typeparam name="THead">头信息类型</typeparam>
        /// <typeparam name="TItem">清单项类型</typeparam>
        /// <returns></returns>
        Task<IOperationResult<IManifest<THead, TItem>>> ReadAsync<THead, TItem>()
            where THead : IManifestHead
            where TItem : IManifestItem;
    }

    /// <summary>
    /// 声明头信息类型与清单项类型的清单文件读取器
    /// </summary>
    /// <typeparam name="THead"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    public interface IManifestFileReader<THead, TItem> : IManifestFileReader
            where THead : IManifestHead
            where TItem : IManifestItem
    {
    }

    public static class IManifestFileReaderExtensions
    {
        /// <summary>
        /// 将文件读取为目标类型的清单, 清单的类型为对应接口泛型参数列表中指定的类型
        /// </summary>
        /// <typeparam name="THead"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Task<IOperationResult<IManifest<THead, TItem>>> ReadAsync<THead, TItem>(this IManifestFileReader<THead, TItem> reader)
            where THead : IManifestHead
            where TItem : IManifestItem
        {
            return reader.ReadAsync<THead, TItem>();
        }

        /// <summary>
        /// 检查传入的类型是否与接口泛型参数中的类型相匹配
        /// </summary>
        /// <typeparam name="THead"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="reader"></param>
        /// <param name="headType"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public static bool CheckType<THead, TItem>(this IManifestFileReader<THead, TItem> reader, Type headType, Type itemType)
            where THead : IManifestHead
            where TItem : IManifestItem
        {
            return headType == typeof(THead) && itemType == typeof(TItem);
        }
        /// <summary>
        /// 检查传入的类型是否与接口泛型参数中的类型相匹配, 如果不匹配, 则抛出异常
        /// </summary>
        /// <typeparam name="THead"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="reader"></param>
        /// <param name="headType"></param>
        /// <param name="itemType"></param>
        public static void CheckTypeThrowException<THead, TItem>(this IManifestFileReader<THead, TItem> reader, Type headType, Type itemType)
            where THead : IManifestHead
            where TItem : IManifestItem
        {
            if (headType != typeof(THead) || itemType != typeof(TItem))
            {
                throw new InvalidOperationException($"传入类型存在与声明类型不匹配的情况, 传入类型: Head => {headType.Name}  Item => {itemType.Name}");
            }
        }
    }
}
