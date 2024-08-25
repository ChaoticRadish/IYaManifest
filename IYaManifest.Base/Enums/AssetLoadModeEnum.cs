using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Enums
{
    /// <summary>
    /// 资源加载模式
    /// </summary>
    public enum AssetLoadModeEnum
    {
        /// <summary>
        /// 直接转换为对象
        /// </summary>
        ToObject,
        /// <summary>
        /// 懒加载资源
        /// </summary>
        LazyAsset,
        /// <summary>
        /// 自定义加载方式
        /// </summary>
        Custom,
    }
}
