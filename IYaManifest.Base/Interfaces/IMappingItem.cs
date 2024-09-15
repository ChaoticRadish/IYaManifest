using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Interfaces
{
    public interface IMappingItem
    {
        /// <summary>
        /// 资源类型名
        /// </summary>
        public string AssetType { get; }
        /// <summary>
        /// 资源类型对应的 <see cref="Type"/>
        /// </summary>
        public Type AssetClass { get; }
        /// <summary>
        /// 资源类型对应读写实现的 <see cref="Type"/>
        /// </summary>
        public Type WriteReadImplClass { get; }
    }
}
