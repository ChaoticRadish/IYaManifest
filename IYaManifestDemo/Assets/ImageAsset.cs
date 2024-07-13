using Common_Util.Data.Struct;
using IYaManifest.Core.Base;
using IYaManifest.Core;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Common_Util.Attributes.General;

namespace IYaManifestDemo.Assets
{
    internal class ImageAsset : IAsset
    {
        public string AssetId { get; set; } = string.Empty;
        public string AssetType => nameof(AssetTypeEnum.Image);

        [InfoToString]
        public BitmapSource? Image { get; set; }

        [InfoToString]
        public BitmapDecoder? BitmapDecoder { get; set; }
    }

    internal class ImageAssetWriteReadImpl : AssetWriteReadImplBase<ImageAsset>
    {
        public override IOperationResult<ImageAsset> LoadFrom(Stream stream)
        {
            OperationResult<ImageAsset> result;
            try
            {
                BitmapFrame image = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.None);

                return result = new ImageAsset()
                {
                    Image = image,
                    BitmapDecoder = image.Decoder,
                };
            }
            catch (Exception ex)
            {
                OperationResultEx<ImageAsset> exResult = ex;
                return exResult;
            }
        }

        public override IOperationResult<ImageAsset> LoadFrom(byte[] data)
        {
            using MemoryStream ms = new MemoryStream(data);
            return LoadFrom(ms);
        }

        public override IOperationResult<byte[]> Serialization(ImageAsset asset)
        {
            using MemoryStream ms = new();
            var result = WriteTo(asset, ms);
            if (result.IsSuccess)
            {
                return OperationResultHelper.Failure<OperationResult<byte[]>>(result, null);
            }
            return (OperationResult<byte[]>)ms.ToArray();
        }

        public override IOperationResult WriteTo(ImageAsset asset, Stream stream)
        {
            OperationResult<byte[]> result;
            if (asset.BitmapDecoder == null || asset.Image == null)
            {
                return result = Array.Empty<byte>();
            }
            var encoder = BitmapEncoder.Create(asset.BitmapDecoder.CodecInfo.ContainerFormat);
            encoder.Frames.Add(BitmapFrame.Create(asset.Image));
            encoder.Save(stream);
            return (OperationResult)true;
        }
    }
}
