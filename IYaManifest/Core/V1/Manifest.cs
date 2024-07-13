using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Core.V1
{
    public class Manifest : IManifest<ManifestHead, ManifestItem>
    {
        public required ManifestHead Head { get; set; }
        public ManifestItem[] Items { get; set; } = [];
    }
}
