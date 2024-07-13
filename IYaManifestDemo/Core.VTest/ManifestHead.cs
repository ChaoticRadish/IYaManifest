using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifestDemo.Core.VTest
{
    internal class ManifestHead : IManifestHead
    {
        public string Package { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Remark { get; set; } = string.Empty;

        public DateTime CreateTime { get; set; } = default;
    }
}
