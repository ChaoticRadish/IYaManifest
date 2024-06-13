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
        /// 保存在清单内
        /// </summary>
        InManifest,
        /// <summary>
        /// 其他的存储于外部的方式
        /// </summary>
        Outside,
    }
}
