using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Core.Base
{
    /// <summary>
    /// 用于占位的空资源
    /// </summary>
    public class EmptyAsset : IAsset
    {
        public string AssetId { get; set; } = string.Empty;

        public string AssetType { get; set; } = string.Empty;
    }
}
