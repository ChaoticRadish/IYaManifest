using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Interfaces
{
    /// <summary>
    /// 清单的头信息
    /// </summary>
    public interface IManifestHead
    {
        /// <summary>
        /// 清单包名
        /// </summary>
        string Package { get; }
        /// <summary>
        /// 清单版本号
        /// </summary>
        string Version { get; }
        /// <summary>
        /// 清单名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 清单资源的备注信息
        /// </summary>
        string Remark { get; }
        /// <summary>
        /// 清单文件的制成时间
        /// </summary>
        DateTime CreateTime { get; }
    }
}
