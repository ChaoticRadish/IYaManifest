using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Interfaces
{
    /// <summary>
    /// 资源
    /// </summary>
    public interface IAsset
    {
        /// <summary>
        /// 资源 ID
        /// </summary>
        string AssetId { get; set; }
        /// <summary>
        /// 资源类型 (字符串)
        /// </summary>
        string AssetType { get; }
    }

    /// <summary>
    /// 资源类型是枚举值的资源
    /// </summary>
    /// <typeparam name="TAssetTypeEnum"></typeparam>
    public abstract class AssetBase<TAssetTypeEnum> : IAsset
        where TAssetTypeEnum : Enum
    {
        public abstract string AssetId { get; set; }
        string IAsset.AssetType => AssetType.ToString();
        /// <summary>
        /// 资源类型
        /// </summary>
        public TAssetTypeEnum AssetType { get; set; } = default!;


    }
}
