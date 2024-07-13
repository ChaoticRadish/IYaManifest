using Common_Util.Data.Exceptions;
using Common_Util.Extensions;
using Common_Util.IO;
using Common_Util.Module.Command;
using Common_Util.Streams;
using IYaManifest;
using IYaManifest.Core;
using IYaManifest.Core.Base;
using IYaManifest.Core.V1;
using IYaManifest.Interfaces;
using IYaManifestDemo.Assets;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IYaManifestDemo.TestPages
{
    /// <summary>
    /// TestManifestFile002.xaml 的交互逻辑
    /// </summary>
    public partial class TestManifestFile002 : Page
    {
        public TestManifestFile002()
        {
            InitializeComponent();

            ViewModel = new();
            this.DataContext = ViewModel;
        }
        private TestViewModel_ManifestFile002 ViewModel { get; set; }
        
    }

    public class TestViewModel_ManifestFile002 : TestViewModelBase
    {
        private string fileName = string.Empty;

        public string FileName { get => fileName; set { fileName = value; OnPropertyChanged(); } }

        #region 清单内容编辑 

        private ObservableCollection<ManifestItem> manifestItems = [];
        public ObservableCollection<ManifestItem> ManifestItems
        {
            get => manifestItems;
            set
            {
                manifestItems = value;
                OnPropertyChanged();
            }
        }

        public ICommand ClearCommand => new SampleCommand(_ => clear(), _ => true);

        private void clear()
        {
            ManifestItems.Clear();
        }


        public ICommand RandomTestText1Command => new SampleCommand(_ => randomTestText1(), _ => true);
        private void randomTestText1()
        {
            add(new TestTextAsset1() { Data = Common_Util.Random.RandomStringHelper.GetRandomEnglishString(100), }, 
                "RandomText1", IYaManifest.Enums.AssetDataStorageModeEnum.ManifestData);
        }

        public ICommand RandomTestText2Command => new SampleCommand(_ => randomTestText2(), _ => true);
        private void randomTestText2()
        {
            add(new TestTextAsset2() { Data = Common_Util.Random.RandomStringHelper.GetRandomEnglishString(100), },
                "RandomText2", IYaManifest.Enums.AssetDataStorageModeEnum.ManifestData);
        }

        public ICommand RandomTestText3Command => new SampleCommand(_ => randomTestText3(), _ => true);
        private void randomTestText3()
        {
            add(new TestTextAsset1() { Data = Common_Util.Random.RandomStringHelper.GetRandomEnglishString(100), },
                "RandomText3", IYaManifest.Enums.AssetDataStorageModeEnum.InManifest);
        }

        public ICommand RandomTestText4Command => new SampleCommand(_ => randomTestText4(), _ => true);
        private void randomTestText4()
        {
            add(new TestTextAsset2() { Data = Common_Util.Random.RandomStringHelper.GetRandomEnglishString(100), },
                "RandomText4", IYaManifest.Enums.AssetDataStorageModeEnum.InManifest);
        }


        public ICommand AddImageCommand => new SampleCommand(_ => addImage(), _ => true);

        private void addImage()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                string path = dialog.FileName;
                FileInfo fi = new FileInfo(path);
                addOutside("Image", nameof(AssetTypeEnum.Image), path, new LazyImageAsset()
                {
                    FileName = path,
                    Start = 0,
                    Length = (uint)fi.Length,
                    ReadImpl = new ImageAssetWriteReadImpl(),
                    AssetType = nameof(AssetTypeEnum.Image),
                });
            }
        }


        Dictionary<string, int> ids = [];
        private string nextId(string group)
        {
            if (!ids.TryGetValue(group, out _))
            {
                ids.Add(group, 0);
            }
            else
            {
                ids[group]++;
            }
            return $"{group}:{ids[group]}";

        }
        private void add(IAsset asset, string group, IYaManifest.Enums.AssetDataStorageModeEnum storageMode, string? outsidePath = null)
        {
            ManifestItems.Add(new ManifestItem()
            {
                AssetReference = asset,
                AssetId = nextId(group),
                AssetType = asset.AssetType,
                StorageMode = storageMode,
                OutsidePath = outsidePath,
            });
        }
        private void addOutside(string group, string assetType, string outsidePath, IAsset? assetRef = null)
        {
            if (FileName.IsEmpty())
            {
                Globals.TestLogger?.Warning("尝试添加外部存储的清单项失败, 没有提供清单文件的目录, 无法计算相对路径! ");
                return;
            }
            ManifestItems.Add(new ManifestItem()
            {
                AssetReference = assetRef ?? new EmptyAsset(),
                AssetId = nextId(group),
                AssetType = assetType,
                StorageMode = IYaManifest.Enums.AssetDataStorageModeEnum.Outside,
                OutsidePath = PathHelper.GetRelativelyPath(FileName, outsidePath),
            });
        }
        #endregion

        #region 创建文件

        public bool IsCreateFileException { get => CreateFileException != null; }

        private Exception? createFileException { get; set; }
        public Exception? CreateFileException 
        { 
            get => createFileException;
            set
            { 
                createFileException = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCreateFileException));
            }
        }

        public ICommand CreateFileCommand => new SampleCommand(_ => _ = createFile(), _ => true);

        private async Task createFile()
        {
            CreateFileException = null;
            try
            {
                ManifestFileCreator creator = new()
                {
                    AppMark = 42,
                    AssetWriteReader = new(),
                    Manifest = new()
                    {
                        Head = new()
                        {
                            Name = "测试清单",
                            Package = "清单:TestManifestFile002",
                            CreateTime = DateTime.Now,
                            Remark = "备注备注备注",
                            Version = "1.0.0",
                        },
                        Items = [.. ManifestItems],
                    },

                };

                await creator.CreateAsync(fileName);
            }
            catch (Exception ex) 
            {
                CreateFileException = ex;
            }

        }
        #endregion


        #region 读取文件

        private string fullManifestText = string.Empty;
        public string FullManifestText { get => fullManifestText; set { fullManifestText = value; OnPropertyChanged(); } }

        private string manifestFileHeadText = string.Empty;
        public string ManifestFileHeadText { get => manifestFileHeadText; set { manifestFileHeadText = value; OnPropertyChanged(); } }

        private string readResult = string.Empty;
        public string ReadResult { get => readResult; set { readResult = value; OnPropertyChanged(); } }


        private ObservableCollection<ManifestItem> readManifestItems = [];
        public ObservableCollection<ManifestItem> ReadManifestItems
        {
            get => readManifestItems;
            set
            {
                readManifestItems = value;
                OnPropertyChanged();
            }
        }


        public bool IsReadFileException { get => ReadFileException != null; }

        private Exception? readFileException { get; set; }
        public Exception? ReadFileException
        {
            get => readFileException;
            set
            {
                readFileException = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsReadFileException));
            }
        }

        public ICommand ReadFileCommand => new SampleCommand(_ => _ = readFile(), _ => true);

        private async Task readFile()
        {
            ReadFileException = null;

            FullManifestText = string.Empty;
            ManifestFileHeadText = string.Empty;
            ReadResult = string.Empty;

            try
            {
                var headResult = await ManifestFileReadHelper.TryReadHeadAsync(FileName);
                if (headResult.IsFailure)
                {
                    throw new OperationFailureException(headResult);
                }

                ManifestFileHeadText = headResult.Data.FullInfoString();

                using (FileStream fs = File.OpenRead(FileName))
                {
                    using (OffsetWrapperStream ows = new(fs, headResult.Data.ManifestStart, headResult.Data.ManifestLength))
                    {
                        using (StreamReader sr = new StreamReader(ows, Encoding.UTF8))
                        {
                            FullManifestText = sr.ReadToEnd();
                        }
                    }
                    fs.Seek(0, SeekOrigin.Begin);
                    var resultResult = await ManifestFileReadHelper.TryReadAsync<ManifestHead, ManifestItem>(FileName);
                    ReadResult = resultResult.FullInfoString();

                    ReadManifestItems.Clear();
                    if (resultResult.Data != null)
                    {
                        foreach (var item in resultResult.Data.Items)
                        {
                            ReadManifestItems.Add(item);
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                ReadFileException = ex;
            }

        }
        #endregion
    }
}
