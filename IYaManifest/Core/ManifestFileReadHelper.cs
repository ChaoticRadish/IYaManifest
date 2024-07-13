using Common_Util.Data.Struct;
using Common_Util.Data.Structure.Value;
using Common_Util.Streams;
using IYaManifest.Defines;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;


namespace IYaManifest.Core
{
    public static class ManifestFileReadHelper
    {
        private static Dictionary<(byte version, uint? appMark), ReadImpl> implConfigs = [];


        public struct ReadImpl
        {
            /// <summary>
            /// 实现对应版本
            /// </summary>
            public required byte Version { get; set; }
            /// <summary>
            /// 对应的App标记, 如果为空, 表示允许任意 App 
            /// </summary>
            public required uint? AppMark { get; set; }

            /// <summary>
            /// 实现的 key 值
            /// </summary>
            internal readonly (byte version, uint? appMark) Key => (Version, AppMark);

            /// <summary>
            /// 取得对应读取器对象的方法
            /// </summary>
            public required Func<ReadContext, IManifestFileReader> GetReaderFunc { get; set; }
        }

        public struct ReadContext
        {
            /// <summary>
            /// 清单文件路径
            /// </summary>
            public required string FilePath { get; set; }

            /// <summary>
            /// 清单文件的文件头
            /// </summary>
            public required ManifestFileHead FileHead { get; set; }

            /// <summary>
            /// 清单文件内容偏移量
            /// </summary>
            public int ContentOffset { get; set; } 
        }

        #region 对外方法

        #region 读取时的固定值

        private static readonly int headSize = Marshal.SizeOf(typeof(ManifestFileHead));
        #endregion

        /// <summary>
        /// 设置实现方法, 如果已有相同版本号的实现设定, 则覆盖. 
        /// </summary>
        /// <param name="impl"></param>
        public static void SetImpl(ReadImpl impl)
        {
            var key = (impl.Version, impl.AppMark);
            if (!implConfigs.TryAdd(key, impl))
            {
                implConfigs[key] = impl;
            }
        }
        /// <summary>
        /// 获取输入参数对应的实现方法
        /// </summary>
        /// <param name="version"></param>
        /// <param name="appMark"></param>
        /// <returns></returns>
        private static IOperationResult<ReadImpl> GetImpl(byte version, uint appMark)
        {
            OperationResult<ReadImpl> result;

            var sampleVersionKeys = implConfigs.Where(i => i.Key.version == version).Select(i => i.Key).ToArray();
            (byte version, uint? appMark) key;
            if (sampleVersionKeys.Any(i => i.appMark == appMark))
            {
                key = sampleVersionKeys.First(i => i.appMark == appMark);
            }
            else if (sampleVersionKeys.Any(i => i.appMark == null))
            {
                key = sampleVersionKeys.First(i => i.appMark == null);
            }
            else
            {
                return result = $"未能找到符合条件的读写实现, version={version}, appMark={appMark}";
            } 
            return result = implConfigs[key];

        }


        /// <summary>
        /// 尝试读取清单文件的文件头
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<IOperationResultEx<ManifestFileHead>> TryReadHeadAsync(string fileName)
        {
            try
            {
                OperationResultEx<ManifestFileHead> defaultTypeOuput;

                if (!Path.Exists(fileName)) return defaultTypeOuput = "文件路径不存在! ";
                using FileStream fileStream = File.OpenRead(fileName);

                var readResult = await _readHeadAsync(fileStream);

                return defaultTypeOuput = (readResult, null);
            }
            catch (Exception ex)
            {
                return OperationResultEx<ManifestFileHead>.Failure(ex);
            }
        }

        /// <summary>
        /// 尝试读取文件为清单
        /// <para>读取时根据读取到的文件头, 获取对应的读取器来读取数据</para>
        /// </summary>
        /// <typeparam name="THead"></typeparam>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<IOperationResultEx<IManifest<THead, TItem>>> TryReadAsync<THead, TItem>(string fileName)
            where THead : IManifestHead
            where TItem : IManifestItem
        {
            return await TryReadAsync<THead, TItem>(new FileSegment(fileName));
        }
        public static async Task<IOperationResultEx<IManifest<THead, TItem>>> TryReadAsync<THead, TItem>(FileSegment segment)
            where THead : IManifestHead
            where TItem : IManifestItem
        {
            try
            {
                OperationResultEx<IManifest<THead, TItem>> defaultTypeOuput;
                var readResult = await _tryReadAsync<THead, TItem>(segment.FullName);
                return defaultTypeOuput = (readResult, null);
            }
            catch (Exception ex)
            {
                return OperationResultEx<IManifest<THead, TItem>>.Failure(ex);
            }
        }
        private static async Task<IOperationResult<IManifest<THead, TItem>>> _tryReadAsync<THead, TItem>(string fileName)
            where THead : IManifestHead
            where TItem : IManifestItem
        {
            return await _tryReadAsync<THead, TItem>(new FileSegment(fileName));
        }
        private static async Task<IOperationResult<IManifest<THead, TItem>>> _tryReadAsync<THead, TItem>(FileSegment segment)
            where THead : IManifestHead
            where TItem : IManifestItem
        {
            OperationResult<IManifest<THead, TItem>> defaultTypeOuput;

            if (!Path.Exists(segment.FullName)) return defaultTypeOuput = "文件路径不存在! ";
            using FileStream fileStream = File.OpenRead(segment.FullName);
            using OffsetWrapperStream ows = new OffsetWrapperStream(fileStream, segment.Start, segment.Length);

            var readHeadResult = await _readHeadAsync(ows);
            if (readHeadResult.IsFailure) return defaultTypeOuput = OperationResultHelper.Failure<OperationResult<IManifest<THead, TItem>>>(readHeadResult, "读取清单文件头");

            var head = readHeadResult.Data;

            var getReadImpl = GetImpl(head.Version, head.AppMark);
            if (getReadImpl.IsFailure) return defaultTypeOuput = OperationResultHelper.Failure<OperationResult<IManifest<THead, TItem>>>(getReadImpl, "获取对应的读取实现");

            var readContext = new ReadContext()
            {
                FileHead = head,
                FilePath = segment.FullName,
            };

            using IManifestFileReader reader = getReadImpl.Data.GetReaderFunc(readContext);

            var result = await reader.ReadAsync<THead, TItem>();
            if (result.IsFailure)
            {
                return defaultTypeOuput = OperationResultHelper.Failure<OperationResult<IManifest<THead, TItem>>>(result, "读取清单内容");
            }

            return result;
        }

        private static async Task<IOperationResult<ManifestFileHead>> _readHeadAsync(Stream stream)
        {
            OperationResult<ManifestFileHead> defaultTypeOuput;

            if (stream.Length <= headSize) return defaultTypeOuput = $"数据流长度 ({stream.Length}) 小于清单文件头的尺寸 ({headSize}), 无法读取! ";

            // 读取数据
            byte[] headData = new byte[headSize];
            stream.Seek(0, SeekOrigin.Begin);
            int readCount = await stream.ReadAsync(headData);
            if (readCount != headSize) return defaultTypeOuput = $"读取文件头时, 读取到的 byte 数 ({readCount}) 与预期长度 ({headSize}) 不匹配! ";

            // 校验
            byte crc = Common_Util.Check.CRCHelper.CRC8(new Span<byte>(headData, 0, headSize - 1));
            if (crc != headData[^1]) return defaultTypeOuput = $"文件头数据校验失败, CRC-8 计算值 ({crc}) != 数据值 ({headData[^1]})";

            // 转换为结构体
            IntPtr headPtr = Marshal.AllocHGlobal(headSize);
            Marshal.Copy(headData, 0, headPtr, headSize);
            var converterResult = Marshal.PtrToStructure(headPtr, typeof(ManifestFileHead));
            if (converterResult == null) return defaultTypeOuput = "数据转换为结构体时, 得到的值为 null ";

            return defaultTypeOuput = (ManifestFileHead)converterResult;

        }

        #endregion
    }
}
