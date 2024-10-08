﻿using Common_Util.IO;
using Common_Wpf.Themes;
using IYaManifest;
using IYaManifest.Core;
using IYaManifest.Core.V1;
using IYaManifest.Extensions;
using IYaManifest.Wpf;
using IYaManifestDemo.AppManage;
using IYaManifestDemo.Assets;
using IYaManifestDemo.Pages.Assets;
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
            //IYaManifest.Core.ManifestFileReadHelper.SetImpl(new IYaManifest.Core.ManifestFileReadHelper.ReadImpl()
            //{
            //    Version = 1,
            //    AppMark = 42,
            //    GetReaderFunc = content =>
            //    {
            //        return new IYaManifest.Core.V1.ManifestFileReader(
            //            IYaManifest.Core.V1.ManifestFileReaderContext.From(content))
            //        {
            //            AssetLoadModeSelector = selector
            //        };
            //    },
            //});
            IYaManifest.Core.ManifestFileReadHelper.SetImpl(new IYaManifest.Core.ManifestFileReadHelper.ReadImpl()
            {
                Version = 1,
                AppMark = null,
                GetReaderFunc = content =>
                {
                    return new IYaManifest.Core.V1.ManifestFileReader(
                        IYaManifest.Core.V1.ManifestFileReaderContext.From(content))
                    {
                        AssetLoadModeSelector = selector
                    };
                },
            });

            setDefaultMapping(AssetTypeEnum.TestText1, 
                typeof(TestTextAsset1), typeof(TestTextAsset1WriteReadImpl),
                displayerPageClass: typeof(TestTextAsset1DisplayerPage));
            setDefaultMapping(AssetTypeEnum.TestText2,
                typeof(TestTextAsset2), typeof(TestTextAsset2WriteReadImpl),
                displayerPageClass: typeof(TestTextAsset2DisplayerPage),
                editorPageClass: typeof(TestTextAsset2EditorPage));
            setDefaultMapping(AssetTypeEnum.Image,
                typeof(ImageAsset), typeof(ImageAssetWriteReadImpl), 
                displayerPageClass: typeof(ImageAssetDisplayerPage),
                editorPageClass: typeof(ImageAssetEditorPage));

            MappingHelper.LoadAllExDll(Common_Util.Enums.AppendConflictDealMode.Ignore, null);
        }

        private void setDefaultMapping(
            AssetTypeEnum assetType, Type assetClass, Type writeReadImplClass, 
            Type? creatorPageClass = null, Type? displayerPageClass = null, Type? editorPageClass = null)
        {
            AssetWriteReader.DefaultMapping.Set(assetType,
                new()
                {
                    AssetClass = assetClass,
                    WriteReadImplClass = writeReadImplClass,
                });
            if (creatorPageClass != null || displayerPageClass != null)
            {
                PageTypeMapManager.Instance.Add(
                    PageTypeMappingItem.Create(
                        assetClass, 
                        creatorPageClass, 
                        displayerPageClass,
                        editorPageClass));
            }
        }

    }

}
