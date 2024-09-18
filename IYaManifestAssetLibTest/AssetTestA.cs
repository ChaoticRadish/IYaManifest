using Common_Util.Data.Struct;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifestAssetLibTest
{
    public class AssetTestA : AssetBase<AssetTypeEnum>, IDataStringAsset
    {
        public AssetTestA()
        {
            AssetType = AssetTypeEnum.TestA;
        }
        public override string AssetId { get; set; } = string.Empty;

        public DataPackage Data { get; set; }
        public struct DataPackage
        {
            public int ValueI { get; set; }

            public float ValueF { get; set; }
        }

        public string DataString { get => $"ValueI: {Data.ValueI}; ValueF: {Data.ValueF}"; }

        public override string ToString()
        {
            return $"[{AssetType}]({AssetId}) ValueI: {Data.ValueI}; ValueF: {Data.ValueF}";
        }
    }

    public class AssetTestAWRImpl : AssetWriteReadImplBaseEx1<AssetTestA>
    {
        private static int dataSize = Marshal.SizeOf(typeof(AssetTestA.DataPackage));

        public override IOperationResult<AssetTestA> LoadFrom(Stream stream)
        {
            nint ptr = Marshal.AllocHGlobal(dataSize);
            for (int i = 0; i < dataSize; i++)
            {
                int b = stream.ReadByte();
                if (b < 0) break;
                Marshal.WriteByte(ptr + i, (byte)b);
            }
            var data = Marshal.PtrToStructure<AssetTestA.DataPackage>(ptr);
            Marshal.FreeHGlobal(ptr);

            return (OperationResult<AssetTestA>)new AssetTestA()
            {
                Data = data,
            };
        }

        public override IOperationResult WriteTo(AssetTestA asset, Stream stream)
        {
            nint ptr = Marshal.AllocHGlobal(dataSize);
            Marshal.StructureToPtr(asset.Data, ptr, false);
            for (int i = 0; i < dataSize; i++) 
            {
                byte b = Marshal.ReadByte(ptr + i);
                stream.WriteByte(b);
            }
            Marshal.FreeHGlobal(ptr);
            return OperationResult.Success;
        }
    }
}
