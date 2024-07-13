using Common_Util.Attributes.Xml;
using Common_Util.Data.Structure.Value;
using IYaManifest.Enums;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IYaManifest.Core.V1
{
    /// <summary>
    /// 默认实现的清单资源项
    /// <para>外部资源使用与清单文件的相对坐标表示, 外部资源无需校验 MD5 !</para>
    /// </summary>
    public class ManifestItem : IManifestItem
    {
        [XmlTextValue]
        public string AssetId { get => AssetCode; set => AssetCode = value; }

        /// <summary>
        /// 资源编号
        /// </summary>
        [XmlIgnore]
        public LayeringAddressCode AssetCode { get; set; }

        [XmlTextValue]
        public string AssetType { get; set; } = string.Empty;

        /// <summary>
        /// 资源引用
        /// </summary>
        [XmlIgnore]
        public required IAsset AssetReference { get; set; }

        [XmlAttribute]
        [XmlTextValue]
        public AssetDataStorageModeEnum StorageMode { get; set; }

        [XmlElement]
        public byte[] MD5 
        {
            get => FixedMD5 ?? md5;
            set => md5 = value ?? []; 
        }
        private byte[] md5 = [];
        /// <summary>
        /// 固定值 MD5 值, 如果不为空, 则 <see cref="MD5"/> 始终返回此值
        /// </summary>
        [XmlElement]
        public byte[]? FixedMD5 { get; set; }

        public long LocationStart { get; set; }
        public long LocationLength { get; set; }


        [XmlTextValue]
        public string? OutsidePath { get; set; }

        #region 序列号常量参数
        public const string INNER_DATA_TAG_NAME = "Data";

        #endregion
    }
    /// <summary>
    /// 默认实现的清单资源项, 其资源类型为泛型参数设置的枚举类型值
    /// </summary>
    /// <typeparam name="TAssetTypeEnum"></typeparam>
    public class ManifestItem<TAssetTypeEnum> : ManifestItem
        where TAssetTypeEnum : Enum
    {
        public required new TAssetTypeEnum AssetType
        { 
            get => Common_Util.EnumHelper.ConvertOrDefault<TAssetTypeEnum>(base.AssetType, default!); 
            set => base.AssetType = value.ToString(); 
        }
    }
}
