using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Interfaces
{
    /// <summary>
    /// 可以以数据字符串的形式表示的资源
    /// </summary>
    public interface IDataStringAsset : IAsset
    {
        public string DataString { get; }
    }
}
