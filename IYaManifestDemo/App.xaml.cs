using Common_Util.IO;
using Common_Wpf.Themes;
using IYaManifest;
using IYaManifest.Core;
using IYaManifest.Core.V1;
using IYaManifestDemo.Assets;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace IYaManifestDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ColorManager.ColorGroup = ColorGroupEnum.Default;

            TempFileHelper.CustomTempFileDir = ".\\Temp";
            if (!Directory.Exists(TempFileHelper.CustomTempFileDir))
            {
                Directory.CreateDirectory(TempFileHelper.CustomTempFileDir);
            }
            var tempFile = TempFileHelper.NewTempFile();

            AssetLoadModeSelector selector = new()
            {
                AssetWriteReader = new(),
                SelectFunc = (item, length) =>
                {
                    if (item.StorageMode == IYaManifest.Enums.AssetDataStorageModeEnum.Outside)
                    {
                        return (IYaManifest.Enums.AssetLoadModeEnum.LazyAsset, string.Empty);
                    }
                    if (item.AssetType == nameof(AssetTypeEnum.TestText2))
                    {
                        return (IYaManifest.Enums.AssetLoadModeEnum.Custom, "test");
                    }
                    return (IYaManifest.Enums.AssetLoadModeEnum.ToObject, string.Empty);
                },
            };
            selector.AddCustom("test", new CacheToStreamLoadHandler(tempFile.Path) { 
                HandleLazyAsset = (asset) =>
                {
                    asset.OnLoad += (_fa, _a) => Globals.TestLogger?.Info($"资源 {_fa.AssetId} 加载");
                    asset.OnUnload += (_fa) => Globals.TestLogger?.Info($"资源 {_fa.AssetId} 卸载");
                    return asset;
                }
            });

            IYaManifest.Core.ManifestFileReadHelper.SetImpl(new IYaManifest.Core.ManifestFileReadHelper.ReadImpl()
            {
                Version = 1,
                AppMark = 42,
                GetReaderFunc = content =>
                {
                    return new IYaManifest.Core.V1.ManifestFileReader(
                        IYaManifest.Core.V1.ManifestFileReaderContext.From(content))
                    {
                        AssetLoadModeSelector = selector
                    };
                },
            });
            AssetWriteReader.DefaultMapping.Set(AssetTypeEnum.TestText1,
                new()
                {
                    AssetClass = typeof(TestTextAsset1),
                    WriteReadImplClass = typeof(TestTextAsset1WriteReadImpl),
                });
            AssetWriteReader.DefaultMapping.Set(AssetTypeEnum.TestText2,
                new()
                {
                    AssetClass = typeof(TestTextAsset2),
                    WriteReadImplClass = typeof(TestTextAsset2WriteReadImpl),
                });
            AssetWriteReader.DefaultMapping.Set(AssetTypeEnum.Image,
                new()
                {
                    AssetClass = typeof(ImageAsset),
                    WriteReadImplClass = typeof(ImageAssetWriteReadImpl),
                });
        }
    }

}
