using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IYaManifest.Defines
{
    /// <summary>
    /// 清单文件头
    /// <para>主要作用: </para>
    /// <para>1. 标识文件类型</para>
    /// <para>2. 标记清单文件内, 清单区域起始位置和数据区域起始位置</para>
    /// <para>3. 标识文件格式版本号, 以及 uint 类型的应用标记</para>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ManifestFileHead
    {
        public const string FIXED_MARK = "IYaMF";
        public static readonly byte[] FixedMarkArray = DefineHelper.CreateFixedMarkArray(FIXED_MARK.AsSpan());

        /// <summary>
        /// 固定标记
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.U1)]
        public byte[] Mark;
        [XmlElement]
        public readonly string MarkAsciiString { get => Encoding.ASCII.GetString(Mark); }

        /// <summary>a
        /// 清单区域起点
        /// </summary>
        public uint ManifestStart;
        public readonly uint ManifestStartValue { get => ManifestStart; }
        /// <summary>
        /// 清单区域长度
        /// </summary>
        public uint ManifestLength;
        public readonly uint ManifestLengthValue { get => ManifestLength; }

        /// <summary>
        /// 16字节 清单区域哈希值 (用于完整性校验)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U1)]
        public byte[] ManifestMd5;
        [XmlElement]
        public readonly byte[] ManifestMd5Value { get => ManifestMd5; }

        /// <summary>
        /// 数据区域起点
        /// </summary>
        public ulong DataStart;
        public readonly ulong DataStartValue { get => DataStart; }
        /// <summary>
        /// 数据区域终点
        /// </summary>
        public ulong DataLength;
        public readonly ulong DataLengthValue { get => DataLength; }

        /// <summary>
        /// 16字节 数据区域哈希值 (用于完整性校验)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16, ArraySubType = UnmanagedType.U1)]
        public byte[] DataMd5;
        [XmlElement]
        public readonly byte[] DataMd5Value { get => DataMd5; }

        /// <summary>
        /// 文件格式版本号
        /// </summary>
        public byte Version;
        public readonly byte VersionValue { get => Version; }

        /// <summary>
        /// 关联应用的标记
        /// </summary>
        public uint AppMark;
        public readonly uint AppMarkValue { get => AppMark; }

        /// <summary>
        /// 文件头的 CRC-8 校验码
        /// </summary>
        public byte CRC8;
        public readonly byte CRC8Value { get => CRC8; }

    }
}
