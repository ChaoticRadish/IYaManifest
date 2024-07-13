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
    internal class TestTextAsset2 : IAsset, ITestTextAsset
    {
        public string AssetId { get; set; } = string.Empty;
        public string AssetType => nameof(AssetTypeEnum.TestText2);

        public string Data { get; set; } = string.Empty;
    }

    internal class TestTextAsset2WriteReadImpl : AssetWriteReadImplBase<TestTextAsset2>
    {
        public override IOperationResult<TestTextAsset2> LoadFrom(Stream stream)
        {
            return LoadFrom(AssetWriteReaderHelper.ReadToEnd(stream));
        }

        public override IOperationResult<TestTextAsset2> LoadFrom(byte[] data)
        {
            data = data.Reverse().ToArray();
            TestTextAsset2 output = new TestTextAsset2()
            {
                Data = Encoding.UTF8.GetString(data, 0, data.Length),
            };
            return (OperationResult<TestTextAsset2>)output;
        }

        public override IOperationResult<byte[]> Serialization(TestTextAsset2 asset)
        {
            var bytes = Encoding.UTF8.GetBytes(asset.Data);
            bytes = bytes.Reverse().ToArray();
            return (OperationResult<byte[]>)bytes;
        }

        public override IOperationResult WriteTo(TestTextAsset2 asset, Stream stream)
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
