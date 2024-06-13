using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Interfaces
{
    public interface IManifest<THead, TItem> 
        where THead : IManifestHead
        where TItem : IManifestItem
    {
        /// <summary>
        /// 清单的头数据
        /// </summary>
        THead Head { get; set; }

        /// <summary>
        /// 清单的所有子项
        /// </summary>
        TItem[] Items { get; set; }
    }
}
