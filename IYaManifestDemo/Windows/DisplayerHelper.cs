﻿using Common_Util.Log;
using IYaManifest.Interfaces;
using IYaManifest.Wpf;
using IYaManifest.Wpf.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifestDemo.Windows
{
    internal static class DisplayerHelper
    {
        public static void ShowAssetDetail(
            Action<Action> uiRun, IAsset asset, 
            ILevelLogger? operationLogger, ILevelLogger? trackLogger)
        {
            if (asset is ILazyAsset lazyAsset)
            {
                showLazyAsset(uiRun, lazyAsset, operationLogger, trackLogger);
            }
            else
            {
                showAsset(uiRun, asset, operationLogger, trackLogger);
            }
        }
        private static void showLazyAsset(Action<Action> uiRun, ILazyAsset lazyAsset,
            ILevelLogger? operationLogger, ILevelLogger? trackLogger)
        {
            operationLogger?.Info("显示懒加载资源: " + lazyAsset.ToString());
            bool loadMark = false;
            if (!lazyAsset.Loaded)
            {
                lazyAsset.Load();
            }
            IAsset? asset = lazyAsset.Asset;
            if (asset == null)
            {
                trackLogger?.Warning("懒加载资源加载后, 资源引用仍为 null 值");
            }
            else
            {
                showAsset(uiRun, asset, operationLogger, trackLogger);
            }
            if (loadMark)
            {
                lazyAsset.Unload();
            }
        }
        private static void showAsset(Action<Action> uiRun, IAsset asset,
            ILevelLogger? operationLogger, ILevelLogger? trackLogger)
        {
            operationLogger?.Info("显示普通资源: " + asset.ToString());

            if (PageTypeMapManager.Instance.TryGet(asset.GetType(), out var mappingItem))
            {
                IAssetDisplayer? displayer = null;
                if (mappingItem.DisplayerType == null)
                {
                    uiRun(() =>
                    {
                        displayer = new CommonDisplayerWindow()
                        {
                            Showing = asset,
                            Title = $"创建 {asset.AssetType ?? "<null>"} 资源 ::: {mappingItem.AssetType.Name}"
                        };
                    });
                }
                else
                {
                    uiRun(() =>
                    {
                        displayer = new CommonAssetDisplayerWindow01()
                        {
                            Showing = asset,
                            Title = $"创建 {asset.AssetType ?? "<null>"} 资源 ::: {mappingItem.AssetType.Name}"
                        };
                    });
                }
                if (displayer == null)
                {
                    trackLogger?.Warning("未能根据资源取得对应的展示器");
                    return;
                }

                displayer.Display();

            }
        }
    }
}
