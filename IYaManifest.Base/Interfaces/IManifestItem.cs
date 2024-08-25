using IYaManifest.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Interfaces
{
    /// <summary>
    /// 清单项
    /// </summary>
    public interface IManifestItem
    {
        /// <summary>
        /// 资源ID
        /// </summary>
        string AssetId { get; set; }

        /// <summary>
        /// 资源类型
        /// </summary>
        string AssetType { get; }

        /// <summary>
        /// 资源数据存储方式
        /// </summary>
        AssetDataStorageModeEnum StorageMode { get; set; }

        /// <summary>
        /// 资源数据的 MD5 值, 如果为空, 则表示不作校验
        /// </summary>
        byte[] MD5 { get; }


        #region 如果是在清单文件内

        /// <summary>
        /// 在清单文件内的开始位置
        /// </summary>
        long LocationStart { get; set; }
        /// <summary>
        /// 在清单文件内的占用长度
        /// </summary>
        long LocationLength { get; set; }

        #endregion
    }
}
