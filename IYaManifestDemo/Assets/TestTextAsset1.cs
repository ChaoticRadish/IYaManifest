using Common_Util.Data.Struct;
using IYaManifest.Core;
using IYaManifest.Core.Base;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifestDemo.Assets
{
    public class TestTextAsset1 : IAsset, ITestTextAsset
    {
        public string AssetId { get; set; } = string.Empty;
        public string AssetType => nameof(AssetTypeEnum.TestText1);

        public string Data { get; set; } = string.Empty;

        public string DataString => Data;
    }

    public class TestTextAsset1WriteReadImpl : AssetWriteReadImplBase<TestTextAsset1>
    {
        public override IOperationResult<TestTextAsset1> LoadFrom(Stream stream)
        {
            return LoadFrom(AssetWriteReaderHelper.ReadToEnd(stream));
        }

        public override IOperationResult<TestTextAsset1> LoadFrom(byte[] data)
        {
            TestTextAsset1 output = new TestTextAsset1()
            {
                Data = Encoding.UTF8.GetString(data, 0, data.Length),
            };
            return (OperationResult<TestTextAsset1>)output;
        }

        public override IOperationResult<byte[]> Serialization(TestTextAsset1 asset)
        {
            var bytes = Encoding.UTF8.GetBytes(asset.Data);
            return (OperationResult<byte[]>)bytes;
        }

        public override IOperationResult WriteTo(TestTextAsset1 asset, Stream stream)
        {
            var bytes = Serialization(asset);
            if (bytes.IsFailure)
            {
                return OperationResultHelper.Failure<OperationResult>(bytes, "资源序列化失败");
            }
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(bytes.Data);
            return (OperationResult)true;
        }
    }
}
