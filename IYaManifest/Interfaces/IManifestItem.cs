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
        /// 资源类型
        /// </summary>
        string AssetType { get; }

        /// <summary>
        /// 资源数据存储方式
        /// </summary>
        AssetDataStorageModeEnum StorageModel { get; set; }

        /// <summary>
        /// 资源数据的 MD5 值
        /// </summary>
        byte[] MD5 { get; }

        #region 如果是在清单文件内

        /// <summary>
        /// 清单文件的开始位置
        /// </summary>
        int LocationStart { get; set; }
        /// <summary>
        /// 清单文件内的占用长度
        /// </summary>
        int LocationLength { get; set; }

        #endregion
    }
}
