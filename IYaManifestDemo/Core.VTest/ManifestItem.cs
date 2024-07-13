using IYaManifest.Enums;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifestDemo.Core.VTest
{
    internal class ManifestItem : IManifestItem
    {
        public string AssetId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string AssetType { get; set; } = string.Empty;

        public AssetDataStorageModeEnum StorageMode { get; set; } = AssetDataStorageModeEnum.InManifest;

        public byte[] MD5 => throw new NotImplementedException();

        public long LocationStart { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public long LocationLength { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
