using Common_Util.Data.Exceptions;
using Common_Util.Log;
using Common_Util.Module.Command;
using IYaManifest.Core;
using IYaManifest.Core.V1;
using IYaManifest.Defines;
using IYaManifest.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IYaManifestDemo.ViewModel
{
    public class ManifestViewerPageViewModel : ViewModelBase
    {
        public ILevelLogger? OperationLogger { get; set; }
        public ILevelLogger? TrackLogger { get; set; }

        private string fileName { get; set; } = string.Empty;
        public string FileName { get => fileName; private set { fileName = value; OnPropertyChanged(); } }

        public ICommand SelectFileCommand => new SampleCommand(_ => selectFile(), _ => true);
        private void selectFile()
        {
            OpenFileDialog dialog = new()
            {
                Multiselect = false,
            };
            if (dialog.ShowDialog() == true)
            {
                OperationLogger?.Info("选择文件: " + dialog.FileName);
                FileName = dialog.FileName;
            }
        }

        public ICommand ReadFileCommand => new SampleCommand(_ => _ = readFile(), _ => true);
        private async Task readFile()
        {
            OperationLogger?.Info("尝试读取文件数据");
            try
            {
                await _readFile();
                TrackLogger?.Info("读取结束");
            }
            catch (Exception ex)
            {
                TrackLogger?.Error("读取过程发生异常", ex);
            }

        }
        private async Task _readFile()
        {
            _resetShowing();
            var headResult = await ManifestFileReadHelper.TryReadHeadAsync(FileName);
            if (headResult.IsFailure)
            {
                TrackLogger?.Warning("读取清单文件头失败, " + headResult.FailureReason);
                return;
            }
            FileHead = headResult.Data;

            var resultResult = await ManifestFileReadHelper.TryReadAsync<ManifestHead, ManifestItem>(FileName);
            if (resultResult.IsFailure)
            {
                TrackLogger?.Warning("读取清单文件为清单对象失败, " + resultResult.FailureReason);
                return;
            }
            if (resultResult.Data == null)
            {
                TrackLogger?.Warning("读取清单文件为清单文件成功, 但是取得 null 值");
                return;
            }
            ManifestHead = resultResult.Data.Head;
            foreach (var item in resultResult.Data.Items)
            {
                ManifestItems.Add(item);
            }
        }
        private void _resetShowing()
        {
            TrackLogger?.Info("重置显示内容");
            FileHead = null;
            ManifestHead = null;
            ManifestItems.Clear();
        }

        public ICommand ItemDetailCommand => new SampleCommand(itemDetail, _ => true);
        private void itemDetail(object? obj)
        {
            if (obj == null)
            {
                SelectedItem = null;
            }
            else if (obj is ManifestItem item)
            {
                SelectedItem = item;
            }
        }

        #region 数据

        private ManifestFileHead? fileHead;
        public ManifestFileHead? FileHead { get => fileHead; set { fileHead = value; OnPropertyChanged(); } }

        private ManifestHead? manifestHead;
        public ManifestHead? ManifestHead { get => manifestHead; set { manifestHead = value; OnPropertyChanged(); } }

        public ObservableCollection<ManifestItem> ManifestItems { get; private set; } = [];


        #endregion

        #region 当前选中

        public bool HasSelected { get => SelectedItem != null; }
        private ManifestItem? selectedItem;
        public ManifestItem? SelectedItem
        {
            get => selectedItem;
            set
            {
                LazyAsset = null;

                selectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelected));

                if (selectedItem?.AssetReference is ILazyAsset lazyAsset)
                {
                    LazyAsset = lazyAsset;
                }

            }
        }
        #region 懒加载资源
        private ILazyAsset? lazyAsset;
        public ILazyAsset? LazyAsset 
        {
            get => lazyAsset;
            set
            {
                if (lazyAsset != null)
                {
                    lazyAsset.LoadedStateChanged -= LazyAsset_LoadedStateChanged;
                }
                lazyAsset = value;
                if (lazyAsset != null)
                {
                    lazyAsset.LoadedStateChanged += LazyAsset_LoadedStateChanged;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(LazyAssetLoaded));
            }
        }

        private void LazyAsset_LoadedStateChanged(object? sender, EventArgs e)
        {
            ILazyAsset? asset = sender as ILazyAsset;
            if (asset != null)
            {
                TrackLogger?.Info($"资源 {asset.AssetId} 加载状态变更, 新状态: {(asset.Loaded ? "已加载" : "未加载")}");
            }

            OnPropertyChanged(nameof(LazyAssetLoaded));
        }

        public bool LazyAssetLoaded { get => lazyAsset?.Loaded ?? false; }
        public IAsset? LazyAssetWrappingAsset { get => lazyAsset?.Asset; }

        public ICommand LazyAssetLoadCommand => new SampleCommand(_ =>
        {
            OperationLogger?.Info("加载当前选择的懒加载资源");
            if (LazyAsset != null)
            {
                LazyAsset.Load();
                OnPropertyChanged(nameof(LazyAssetWrappingAsset));
            }
        }, _ => true);

        public ICommand LazyAssetUnloadCommand => new SampleCommand(_ =>
        {
            OperationLogger?.Info("卸载当前选择的懒加载资源");
            if (LazyAsset != null)
            {
                LazyAsset.Unload();
                OnPropertyChanged(nameof(LazyAssetWrappingAsset));
            }
        },  _ => true);

        #endregion

        #endregion


    }
}
