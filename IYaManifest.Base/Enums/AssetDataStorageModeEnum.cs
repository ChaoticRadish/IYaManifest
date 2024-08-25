using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Enums
{
    /// <summary>
    /// 资源数据的存储方式枚举
    /// </summary>
    public enum AssetDataStorageModeEnum
    {
        /// <summary>
        /// 保存在清单内, 以 base64 编码的形式保存
        /// </summary>
        InManifest,
        /// <summary>
        /// 保存在清单内的数据区域
        /// </summary>
        ManifestData,
        /// <summary>
        /// 其他的存储于外部的方式
        /// </summary>
        Outside,
    }
}
