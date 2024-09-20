using Common_Util.Data.Struct;
using Common_Util.Data.Structure.Tree;
using Common_Util.Data.Structure.Tree.Extensions;
using Common_Util.Data.Structure.Value;
using Common_Util.Data.Structure.Value.Extensions;
using Common_Util.Extensions;
using Common_Util.IO;
using Common_Util.Log;
using Common_Util.Module.Command;
using Common_Util.Streams;
using Common_Util.String;
using IYaManifest.Interfaces;
using IYaManifest.Core;
using IYaManifest.Core.Base;
using IYaManifest.Core.V1;
using IYaManifest.Enums;
using IYaManifest.Wpf;
using IYaManifest.Wpf.Windows;
using IYaManifestDemo.Assets;
using IYaManifestDemo.Windows;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace IYaManifestDemo.ViewModel
{
    public class ManifestCreatorPageViewModel : ViewModelBase
    {
        public ManifestCreatorPageViewModel() 
        {
            UpdateAssetTypes();

            StorageModeOptions.AddEnums();
            Md5SettingOptions.AddEnums();
        }

        public ILevelLogger? OperationLogger { get; set; }
        public ILevelLogger? TrackLogger { get; set; }

        public Action<Action>? UiInvoke { get; set; }
        private void tryUiInvoke(Action action)
        {
            if (UiInvoke != null)
            {
                UiInvoke(action);
            }
            else
            {
                action();
            }
        }

        #region 文件数据
        public uint AppMark { get => appMark; set { appMark = value; OnPropertyChanged(); } }
        private uint appMark;

        private string name = string.Empty;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }


        private string package = string.Empty;
        public string Package
        {
            get => package;
            set
            {
                package = value;
                OnPropertyChanged();
            }
        }

        private string remark = string.Empty;
        public string Remark
        {
            get => remark;
            set
            {
                remark = value;
                OnPropertyChanged();
            }
        }

        private string version = string.Empty;
        public string Version
        {
            get => version;
            set
            {
                version = value;
                OnPropertyChanged();
            }
        }


        #region 清单项列表
        //private ObservableCollection<ManifestItem> manifestItems = [];
        //public ObservableCollection<ManifestItem> ManifestItems
        //{
        //    get => manifestItems;
        //    set
        //    {
        //        manifestItems = value;
        //        OnPropertyChanged();
        //    }
        //}

        private ObservableMultiTree<ManifestUiItem> manifestItemTree = new();
        public ObservableMultiTree<ManifestUiItem> ManifestItemTree
        {
            get => manifestItemTree; set
            {
                manifestItemTree = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 当前需要展示在画面上的顶级节点集合
        /// </summary>
        public ObservableCollection<ObservableMultiTreeNode<ManifestUiItem>> ShowingTopLayerNodes
        {
            get => ManifestItemTree.Root?.Childrens ?? [];
        }

        public class ManifestUiItem : ILayeringAddressCode<string>, IEqualityComparer<ManifestUiItem>, IEquatable<ManifestUiItem>
        {
            private ManifestUiItem() { }
            #region 创建
            public static ManifestUiItem CreateRangeFromPath(string[] path) => new ManifestUiItem()
            {
                Code = new LayeringAddressCode(true, path),
                RelateItem = null,
            };
            public static ManifestUiItem CreateItemFromPath(string[] path) => new ManifestUiItem()
            {
                Code = new LayeringAddressCode(false, path),
                RelateItem = null,
            };
            public static ManifestUiItem CreateItemFromItem(ManifestItem item) => new ManifestUiItem()
            {
                Code = item.AssetCode,
                RelateItem = item,
            };

            #endregion


            public required LayeringAddressCode Code { get; set; }
            public required ManifestItem? RelateItem { get; set; }

            #region 接口实现
            public string[] LayerValues => Code.LayerValues;

            public int LayerCount => Code.LayerCount;

            public bool IsRange => Code.IsRange;
            #endregion

            #region UI相关
            public bool IsItem => !Code.IsRange;

            public bool HasRelateItem { get => RelateItem != null; }
            
            public bool NotHasRelateItem { get => RelateItem == null; }

            public string HeadStr { get => Code.IsAll() ? "[All]" : (Code.IsRange ? "[Range]" : string.Empty); }

            public Brush Background 
            {
                get 
                {
                    const double vMax = 0.4;
                    double v = (-(1 / ((double)Code.LayerCount + 1)) + 1) * vMax;
                    byte vb = (byte)(255 - 255 * vMax + 255 * Math.Min(Math.Max(v, 0), 1));
                    return new SolidColorBrush(Color.FromRgb(vb, vb, vb));
                }
            }

            /// <summary>
            /// 此资源是否允许编辑
            /// </summary>
            public bool AllowEdit
            {
                get
                {
                    if (RelateItem == null) return false;
                    Type? assetType = null;
                    if (RelateItem.AssetReference is ILazyAsset lazyAsset)
                    {
                        assetType = lazyAsset.ExpectReferenceType;
                    }
                    else if (RelateItem.AssetReference is IAsset asset)
                    {
                        assetType = asset.GetType();
                    }
                    if (assetType == null) return false;
                    if (PageTypeMapManager.Instance.TryGet(assetType, out var item))
                    {
                        return item.EditorType != null;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            #endregion
            #region 等价比较

            public bool Equals(ManifestUiItem? x, ManifestUiItem? y)
            {
                return x?.Code == y?.Code;
            }

            public int GetHashCode([DisallowNull] ManifestUiItem obj)
            {
                return obj?.Code.GetHashCode() ?? 0;
            }

            public bool Equals(ManifestUiItem? other)
            {
                return Code == other?.Code;
            }
            #endregion
        }

        #endregion

        #endregion


        #region 操作
        public ICommand ResetCommand => new SampleCommand(_ => Reset(), _ => true);
        public void Reset()
        {

        }

        public ICommand CreateFileCommand => new SampleCommand(_ => CreateFile(), _ => true);
        public void CreateFile()
        {
            _ = createFile();
        }
        private async Task createFile()
        {

            OperationLogger?.Info("创建文件, 选择保存路径");
            SaveFileDialog dialog = new()
            {
                AddExtension = true,
                DefaultExt = ".mf",
            };
            if (dialog.ShowDialog() == true)
            {
                string fileName = dialog.FileName;
                OperationLogger?.Info($"目标路径: {fileName}");

                try
                {
                    TrackLogger?.Info($"开始创建文件");
                    ManifestFileCreator creator = new()
                    {
                        AppMark = AppMark,
                        AssetWriteReader = new(),
                        Manifest = new()
                        {
                            Head = new()
                            {
                                Name = Name,
                                Package = Package,
                                CreateTime = DateTime.Now,
                                Remark = Remark,
                                Version = Version,
                            },
                            Items = ManifestItemTree
                                .Preorder()
                                .Where(i => i.RelateItem != null)
                                .Select(i => i.RelateItem!)
                                .ToArray(),
                        },

                        DealOutsideStorageAsset = ManifestFileCreator.DefaultOutsideStorageAssetHandler(fileName, true),
                    };
                    await creator.CreateAsync(fileName);
                    TrackLogger?.Info($"清单文件创建完成");
                }
                catch (Exception ex)
                {
                    TrackLogger?.Error($"创建文件发生异常", ex);
                }
            }
        }
        #endregion

        #region 清单资源项
        private string? selectedType = null;
        public string? SelectedType 
        {
            get => selectedType; 
            set 
            { 
                selectedType = value; 
                if (value == null)
                {
                    MappingAssetClass = null;
                }
                else
                {
                    if (AssetWriteReader.DefaultMapping.TryGet(value, out var setting))
                    {
                        MappingAssetClass = setting!.AssetClass;
                    }
                    else
                    {
                        MappingAssetClass = null;
                    }
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedType));
            } 
        }
        public bool HasSelectedType { get => selectedType != null; }


        private Type? mappingAssetClass;
        public Type? MappingAssetClass 
        { 
            get => mappingAssetClass; 
            set
            { 
                mappingAssetClass = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMappingAssetClass));
            } 
        }
        public bool HasMappingAssetClass { get => mappingAssetClass != null; }


        private string assetCodeSpace = string.Empty;
        public string AssetCodeSpace 
        { 
            get => assetCodeSpace;
            set 
            { 
                assetCodeSpace = value; 
                OnPropertyChanged();
                updateAssetCode();
            }
        }

        private string assetCodeKey = string.Empty;
        public string AssetCodeKey
        {
            get => assetCodeKey;
            set
            {
                assetCodeKey = value;
                OnPropertyChanged();
                updateAssetCode();
            }
        }

        private void updateAssetCode()
        {
            if (assetCodeKey.IsEmpty())
            {
                AssetCode = new();
            }
            else
            {
                AssetCode = $"{assetCodeSpace}:{assetCodeKey}";
            }
        }
        private LayeringAddressCode assetCode;
        public LayeringAddressCode AssetCode 
        { 
            get => assetCode; 
            set 
            { 
                assetCode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AssetCodeString));
            }
        }
        public string AssetCodeString { get => (string)assetCode; }

        public ObservableCollection<string> AssetTypes { get; } = [];


        public ICommand UpdateAssetTypesCommand => new SampleCommand(_ => UpdateAssetTypes(), _ => true);
        private void UpdateAssetTypes()
        {
            AssetTypes.Clear();
            foreach (var assetType in AssetWriteReader.DefaultMapping.All.Select(i => i.AssetType))
            {
                if (!AssetTypes.Contains(assetType)) AssetTypes.Add(assetType);
            }

        }
        #region 存储相关
        private AssetDataStorageModeEnum storageMode = AssetDataStorageModeEnum.ManifestData;
        public AssetDataStorageModeEnum StorageMode 
        { 
            get => storageMode; 
            set
            { 
                storageMode = value;
                OnPropertyChanged();
            } 
        }
        public ObservableCollection<AssetDataStorageModeEnum> StorageModeOptions { get; } = [];

        private string outsidePathInput = string.Empty;
        public string OutsidePathInput
        {
            get => outsidePathInput;
            set
            {
                outsidePathInput = value;
                OnPropertyChanged();
                checkOutsidePathInput();
            }
        }
        
        private void checkOutsidePathInput()
        {
            OutsidePathCheck = string.Empty;

            if (string.IsNullOrWhiteSpace(OutsidePathInput))
            {
                OutsidePathCheck = "路径为空";
                return;
            }
            try
            {
                string test = PathHelper.GetRelativelyPath(Path.GetPathRoot(AppDomain.CurrentDomain.BaseDirectory)!, OutsidePathInput);
                var checkChars = Path.GetInvalidFileNameChars();
                if (OutsidePathInput.Any(c => (c != '\\' && c != '/' ) && checkChars.Contains(c)))
                {
                    OutsidePathCheck = "路径存在非法字符! ";
                    return;
                }
                //Path.GetFullPath(test);
                new FileInfo(test);
            }
            catch (Exception ex)
            {
                OutsidePathCheck = ex.Message;
            }

        }

        private string outsidePathCheck = string.Empty;
        public string OutsidePathCheck
        {
            get => outsidePathCheck;
            set
            {
                outsidePathCheck = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(OutsidePathError));
            }
        }
        public bool OutsidePathError { get => OutsidePathCheck.IsNotEmpty(); }

        #endregion


        private string itemRemark = string.Empty;
        public string ItemRemark
        {
            get => itemRemark;
            set
            {
                itemRemark = value;
                OnPropertyChanged();
            }
        }

        #region MD5 值
        public enum Md5SettingEnum
        {
            Fixed,
            Auto,
            Empty,
        }
        private Md5SettingEnum md5Setting;
        public Md5SettingEnum Md5Setting
        {
            get => md5Setting;
            set
            {
                md5Setting = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Md5SettingEnum> Md5SettingOptions { get; } = [];

        private byte[] fixedMd5Input = [];
        public byte[] FixedMd5Input
        {
            get => fixedMd5Input;
            set
            {
                fixedMd5Input = value;
                OnPropertyChanged();
            }
        }

        private byte[] autoMd5 = [];
        public byte[] AutoMd5
        {
            get => autoMd5;
            set
            {
                autoMd5 = value;
                OnPropertyChanged();
            }
        }
        public ICommand CreateMd5ValueCommand => new SampleCommand(createMd5Value);
        private void createMd5Value()
        {
            OperationLogger?.Info("生成当前输入资源的 MD5 值以预览");

            TrackLogger?.Info("将当前输入转换为 IAsset");
            var result = inputCoverterToAsset();
            TrackLogger?.Info(result.ToString());
            if (result.IsFailure)
            {
                return;
            }

            TrackLogger?.Info("计算资源 MD5 值");
            var bytesResult = getMd5Value(result.Data!);
            if (bytesResult.IsFailure)
            {
                TrackLogger?.Info($"计算失败: " + bytesResult.FailureReason);
            }
            else if (bytesResult.Data == null)
            {
                TrackLogger?.Info("计算成功, 但返回了 null 数据");
            }
            else
            {
                TrackLogger?.Info($"计算得 MD5 值: {bytesResult.Data.ToHexString()}");
            }
            AutoMd5 = bytesResult.Data ?? [];
        }
        private OperationResultEx<byte[]> getMd5Value(IAsset asset)
        {
            try
            {
                if (asset is ILazyFileAsset lazy)
                {
                    using var fs = File.OpenRead(lazy.FileName);
                    using OffsetWrapperStream ows = new OffsetWrapperStream(fs, lazy.Start, lazy.Length);

                    System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                    byte[] md5Bs = md5.ComputeHash(ows);
                    return md5Bs;
                }
                else
                {
                    if (SelectedType.IsEmpty())
                    {
                        return "资源类型名为空值";
                    }

                    AssetWriteReader awr = new();

                    using MemoryStream ms = new();
                    var wResult = awr.WriteTo(SelectedType!, asset, ms);
                    if (wResult.IsFailure)
                    {
                        return $"资源未能写入内存流, " + wResult.FailureReason;
                    }
                    ms.Seek(0, SeekOrigin.Begin);

                    System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();
                    byte[] md5Bs = md5.ComputeHash(ms);
                    return md5Bs;
                }

            }
            catch (Exception ex)
            {
                return ex;
            }
        }
        #endregion

        #region 特定资源类型输入值
        private string textAssetStringInput = string.Empty;
        public string TextAssetStringInput
        { 
            get => textAssetStringInput;
            set
            {
                textAssetStringInput = value;
                OnPropertyChanged();
            }
        }

        public ICommand ImageAssetSelectImageFile => new SampleCommand(_ => imageAssetSelectImageFile());
        private void imageAssetSelectImageFile()
        {
            OperationLogger?.Info("选择图片文件作为图片资源");
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Multiselect = false,
            };
            if (dialog.ShowDialog() != true)
            {
                OperationLogger?.Info("未选择图片文件");
                return;
            }

            string fileName = dialog.FileName;

            ImageAssetFileName = fileName;
            OnPropertyChanged(nameof(ImageAssetFileName));

            TrackLogger?.Info($"将文件打开为图片资源: {fileName}");
            var result = openFileAsImageAsset(fileName);

            if (!result)
            {
                TrackLogger?.Warning($"未能将文件打开为图片资源: {result.FailureReason}");
            }

            if (result)
            {
                ImageAssetLoadFileResult = $"文件加载成功";
            }
            else
            {
                StringBuilder sb = new("文件加载失败");
                if (result.FailureReason.IsNotEmpty())
                {
                    sb.AppendLine();
                    sb.Append(result.FailureReason);
                }
                if (result.Exception != null)
                {
                    sb.AppendLine();
                    sb.Append(result.Exception);
                }
                ImageAssetLoadFileResult = sb.ToString();
            }
            OnPropertyChanged(nameof(ImageAssetLoadFileResult));

            ImageAsset = result.Data;
            ImageAssetLoadFileSuccess = result.IsSuccess;
            OnPropertyChanged(nameof(ImageAsset));
            OnPropertyChanged(nameof(ImageAssetLoadFileSuccess));




        }
        private OperationResultEx<ImageAsset> openFileAsImageAsset(string fileName)
        {
            try
            {
                using (FileStream fs = File.OpenRead(fileName))
                {
                    ImageAssetWriteReadImpl wrImpl = new();
                    var loadResult = wrImpl.LoadFrom(fs);
                    if (loadResult.IsFailure)
                    {
                        return (loadResult, null);
                    }
                    if (loadResult.Data == null)
                    {
                        return "加载成功, 但仍取得 null 值! ";
                    }
                    return loadResult.Data;

                }
            }
            catch (Exception ex)
            {
                return ex;
            }
        }


        public string ImageAssetFileName { get; set; } = string.Empty;
        public bool ImageAssetLoadFileSuccess { get; set; }
        public ImageAsset? ImageAsset { get; set; }
        public string ImageAssetLoadFileResult { get; set; } = string.Empty;


        #endregion


        #region 树操作

        public ICommand RemoveTreeNodeCommand => new SampleCommand(removeTreeNode);
        private void removeTreeNode(object? obj)
        {
            if (_judgeAssetCode(obj, out var code))
            {
                OperationLogger?.Info($"尝试移除树节点: {code}");
                var removeResult = ManifestItemTree.Remove(code);
                removeResult.Match(
                    () => TrackLogger?.Info("移除成功"),
                    () => TrackLogger?.Warning($"移除失败, {removeResult.FailureReason}"));
            }
        }

        public ICommand AddCurrentInputToList => new SampleCommand(addCurrentInputToList);

        private void addCurrentInputToList()
        {
            OperationLogger?.Info("将当前输入内容添加到清单项列表");
            
            if (SelectedType.IsEmpty())
            {
                TrackLogger?.Warning("未选择资源类型");
                return;
            }


            TrackLogger?.Info("将当前输入转换为 IAsset");
            var result = inputCoverterToAsset();
            TrackLogger?.Info(result.ToString());
            if (result.IsFailure)
            {
                return;
            }

            ManifestItem item = new()
            {
                AssetType = SelectedType!,
                AssetCode = AssetCode,
                StorageMode = StorageMode,
                Remark = ItemRemark,
                AssetReference = result.Data!,
                OutsidePath = OutsidePathInput,
            };
            if (item.AssetCode.IsRange)
            {
                TrackLogger?.Info($"无效的资源编码, 无法使用范围编码值! ");
                return;
            }
            if (item.AssetReference.AssetId.IsEmpty())
            {
                item.AssetReference.AssetId = item.AssetCode;
            }

            switch (Md5Setting)
            {
                case Md5SettingEnum.Fixed:
                    item.FixedMD5 = FixedMd5Input;
                    TrackLogger?.Info("使用固定的 MD5 值: " + FixedMd5Input.ToHexString());
                    break;
                case Md5SettingEnum.Auto:
                    {
                        TrackLogger?.Info("计算资源 MD5 值");
                        var bytesResult = getMd5Value(result.Data!);
                        if (bytesResult.IsFailure)
                        {
                            TrackLogger?.Info($"计算失败: " + bytesResult.FailureReason);
                            return;
                        }
                        else if (bytesResult.Data == null)
                        {
                            TrackLogger?.Info("计算成功, 但返回了 null 数据");
                            return;
                        }
                        else
                        {
                            TrackLogger?.Info($"计算得 MD5 值: {bytesResult.Data.ToHexString()}");
                        }
                        item.MD5 = bytesResult.Data ?? [];
                    }
                    break;
                case Md5SettingEnum.Empty:
                    item.MD5 = [];
                    break;
            }

            var uiItem = ManifestUiItem.CreateItemFromItem(item);
            ManifestItemTree.OrderlyAdd<string, ManifestUiItem>(
                uiItem, 
                (path) => uiItem.Code.PathEquals(path) ? uiItem : ManifestUiItem.CreateItemFromPath(path), 
                ManifestUiItem.CreateRangeFromPath, 
                desc: false,
                comparer: new LayeringAddressCodeComparer<string, ManifestUiItem>());
            TrackLogger?.Info("添加清单项完成: " + item.AssetId);
            //OnPropertyChanged(nameof(ManifestItemTree));
            OnPropertyChanged(nameof(ShowingTopLayerNodes));
        }
        private OperationResultEx<IAsset> inputCoverterToAsset()
        {
            if (MappingAssetClass == typeof(TestTextAsset1))
            {
                return new TestTextAsset1()
                {
                    Data = TextAssetStringInput,
                };
            }
            else if (mappingAssetClass == typeof(TestTextAsset2))
            {
                return new TestTextAsset2()
                {
                    Data = TextAssetStringInput,
                };
            }
            else if (mappingAssetClass == typeof(ImageAsset))
            {
                if (ImageAsset == null)
                {
                    return "图片资源未创建";
                }
                else
                {
                    return ImageAsset;
                }
            }
            else
            {
                if (PageCreateAsset == null)
                {
                    return "资源需要打开专门的创建页面编辑创建, 但未创建完成";
                }
                else 
                {
                    return OperationResultEx<IAsset>.Success(PageCreateAsset);
                }
            }
        }

        #endregion

        #endregion


        #region 创建清单项资源
        /// <summary>
        /// 由页面创建而来的资源
        /// </summary>
        public IAsset? PageCreateAsset
        {
            get => pageCreateAsset;
            set
            {
                pageCreateAsset = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DataPreviewString));
            }
        }
        private IAsset? pageCreateAsset = null;
        
        public string? DataPreviewString
        {
            get
            {
                if (PageCreateAsset is IDataStringAsset dataStringAsset)
                {
                    return dataStringAsset.DataString;
                }
                else
                {
                    return null;
                }
            }
        }


        public ICommand OpenAssetCreatorPageCommand => new SampleCommand(OpenAssetCreatorPage);
        private void OpenAssetCreatorPage()
        {
            PageCreateAsset = null;

            CommonAssetCreatorWindow01 window = new()
            {
                TargetAssetType = MappingAssetClass,
                Tag = null,
                Title = $"创建 {SelectedType ?? "<null>"} 资源 ::: { MappingAssetClass?.Name ?? "<null>" }",
            };

            if (window.ShowDialog() == true)
            {
                PageCreateAsset = window.CreatedAsset; 
            }

        }
        #endregion

        #region 清单项详情

        public ICommand ItemDetailCommand => new SampleCommand(itemDetail, _ => true);
        private void itemDetail(object? obj)
        {
            if (_judgeManifestItem(obj, out var item))
            {
                DisplayerHelper.ShowAssetDetail(tryUiInvoke, item.AssetReference, OperationLogger, TrackLogger);
            }
        }


        #endregion

        #region 清单项编辑
        public ICommand ItemEditCommand => new SampleCommand(itemEdit, _ => true);
        private void itemEdit(object? obj)
        {
            if (_judgeManifestItem(obj, out var item))
            {
                if (editAsset(item.AssetReference, out var newOne))
                {
                    item.AssetReference = newOne;
                }
            }
        }

        private bool editAsset(IAsset asset, [NotNullWhen(true)] out IAsset? newAsset)
        {
            OperationLogger?.Info("编辑资源: " + asset.ToString());

            bool editDone = false;
            IAsset? newOne = null;
            if (PageTypeMapManager.Instance.TryGet(asset.GetType(), out var mappingItem))
            {
                if (mappingItem.EditorType == null)
                {
                    TrackLogger?.Warning("未能根据资源取得对应的编辑器器");
                }
                else
                {
                    tryUiInvoke(() =>
                    {
                        var window = new CommonAssetEditorWindow01()
                        {
                            Input = asset,
                        };
                        if (window.ShowDialog() == true)
                        {
                            newOne = window.Output;
                            editDone = true;
                            OperationLogger?.Info("编辑完成, 取得新数据");
                        }
                        else
                        {
                            OperationLogger?.Info("编辑取消");
                        }
                    });
                }


            }
            newAsset = newOne;
            return editDone;
        }
        #endregion

        #region 私有方法
        private bool _judgeManifestItem(object? obj, [NotNullWhen(true)] out ManifestItem? item)
        {
            item = null;
            if (obj is null) return false;
            else if (obj is ManifestItem _item)
            {
                item = _item;
                return true;
            }
            else if (obj is ManifestUiItem uiItem)
            {
                item = uiItem.RelateItem;
                return item != null;
            }

            return false;
        }
        private bool _judgeAssetCode(object? obj, [NotNullWhen(true)] out ILayeringAddressCode<string>? code)
        {
            code = null;
            if (obj is null) return false;
            else if (obj is ManifestItem _item)
            {
                code = _item.AssetCode;
                return true;
            }
            else if (obj is ManifestUiItem uiItem)
            {
                code = uiItem.Code;
                return true;
            }

            return false;
        }
        #endregion
    }
}
