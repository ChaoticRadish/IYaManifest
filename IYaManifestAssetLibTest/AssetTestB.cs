using Common_Util.Data.Struct;
using Common_Util.Extensions;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifestAssetLibTest
{
    public class AssetTestB : AssetBase<AssetTypeEnum>, IDataStringAsset
    {
        public AssetTestB()
        {
            AssetType = AssetTypeEnum.TestB;
        }
        public override string AssetId { get; set; } = string.Empty;

        public byte[] Data { get; set; } = [];

        public string DataString { get => (Data == null || Data.Length == 0) ? "Empty!" : Data.ToHexString(); }

        public override string ToString()
        {
            return $"[{AssetType}]({AssetId}) {Data.ToHexString()}";
        }
    }

    public class AssetTestBWRImpl : AssetWriteReadImplBaseEx1<AssetTestB>
    {
        public override IOperationResult<AssetTestB> LoadFrom(Stream stream)
        {
            using MemoryStream ms = new();
            byte[] buffer = new byte[1024];
            int count;
            while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, count);
            }
            AssetTestB obj = new()
            {
                Data = ms.ToArray()
            };
            return (OperationResult<AssetTestB>)obj;
        }

        public override IOperationResult WriteTo(AssetTestB asset, Stream stream)
        {
            stream.Write(asset.Data);
            return OperationResult.Success;
        }
    }
}
