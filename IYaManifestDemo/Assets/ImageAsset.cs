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
using Common_Util.IO;

namespace IYaManifestDemo.Assets
{
    public class ImageAsset : IAsset
    {
        public string AssetId { get; set; } = string.Empty;
        public string AssetType => nameof(AssetTypeEnum.Image);

        [InfoToString]
        public BitmapSource? Image { get; set; }

        [InfoToString]
        public BitmapDecoder? BitmapDecoder { get; set; }

        public TempFileHelper.TempFile TempFile { get; set; }

        ~ImageAsset()
        {
            TempFile.Dispose();
        }
    }

    public class ImageAssetWriteReadImpl : AssetWriteReadImplBase<ImageAsset>
    {
        public override IOperationResult<ImageAsset> LoadFrom(Stream stream)
        {
            OperationResult<ImageAsset> result;
            TempFileHelper.TempFile? tempFile = null;
            try
            {
                tempFile = TempFileHelper.NewTempFile();
                using (var writeStream = tempFile.Value.OpenStream())
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(writeStream);
                }
                var readStream = new FileStream(tempFile.Value.Path, FileMode.Open, FileAccess.Read);
                BitmapFrame image;
                try
                {
                    image = BitmapFrame.Create(readStream, BitmapCreateOptions.None, BitmapCacheOption.None);
                }
                catch
                {
                    readStream.Dispose();
                    throw;
                }

                return result = new ImageAsset()
                {
                    Image = image,
                    BitmapDecoder = image.Decoder,
                    TempFile = tempFile.Value,
                };
            }
            catch (Exception ex)
            {
                OperationResultEx<ImageAsset> exResult = ex;
                tempFile?.Dispose();
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
            OperationResult result;
            if (asset.BitmapDecoder == null || asset.Image == null)
            {
                return result = true;
            }
            using var readStream = new FileStream(asset.TempFile.Path, FileMode.Open, FileAccess.Read);
            readStream.CopyTo(stream);
            //var encoder = BitmapEncoder.Create(asset.BitmapDecoder.CodecInfo.ContainerFormat);
            //encoder.Frames.Add(BitmapFrame.Create(asset.Image));
            //encoder.Save(stream);

            return result = true;
        }
    }
}
