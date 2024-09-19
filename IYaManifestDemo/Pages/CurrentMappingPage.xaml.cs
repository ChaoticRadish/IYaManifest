using Common_Util.Log;
using Common_Util.Module.Command;
using IYaManifest.Core;
using IYaManifest.Interfaces;
using IYaManifest.Wpf;
using IYaManifestDemo.AppManage;
using IYaManifestDemo.ViewModel;
using System;
using System.Collections.Generic;
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

namespace IYaManifestDemo.Pages
{
    /// <summary>
    /// CurrentMappingPage.xaml 的交互逻辑
    /// </summary>
    public partial class CurrentMappingPage : Page
    {
        public CurrentMappingPage()
        {
            InitializeComponent();

            ViewModel = new();
            ViewModel.OperationLogger = LevelLoggerHelper.LogTo(LogShower, "操作", string.Empty);
            ViewModel.TrackLogger = LevelLoggerHelper.LogTo(LogShower, "执行过程", string.Empty);

            DataContext = ViewModel;
        }
    
        public CurrentMappingPageViewModel ViewModel { get; }
    }

    public sealed class CurrentMappingPageViewModel : ViewModelBase
    {
        public CurrentMappingPageViewModel()
        {
            refresh();
        }

        public ILevelLogger? OperationLogger { get; set; }
        public ILevelLogger? TrackLogger { get; set; }


        public ICommand RefreshCommand => new SampleCommand(refresh);
        public ICommand ReloadCommand => new SampleCommand(reload);

        private void refresh()
        {
            OperationLogger?.Info("刷新当前展示映射关系");

            var writeReadMapping = AssetWriteReader.DefaultMapping.All;
            var pageMapping = PageTypeMapManager.Instance.All;

            List<DisplayItem> items = new();
            foreach (var mapping in writeReadMapping)
            {
                items.Add(new DisplayItem()
                {
                    AssetClass = mapping.AssetClass,
                    AssetType = mapping.AssetType,
                    WriteReadImplClass = mapping.WriteReadImplClass,
                    PageTypeMappingItems = pageMapping.TryGetValue(mapping.AssetClass, out var pageMappingItems) ? 
                        pageMappingItems.Select(i => new PageTypeMappingItem(i)).ToArray() 
                        : 
                        [],
                });
            }
            DisplayItems = items.ToArray();
        }
        private void reload()
        {
            OperationLogger?.Info("重新加载扩展 DLL 文件");
            
            MappingHelper.LoadAllExDll(Common_Util.Enums.AppendConflictDealMode.Override, TrackLogger);
            refresh();
        }


        public DisplayItem[] DisplayItems
        {
            get => displayItems; set 
            {
                displayItems = value ?? [];
                OnPropertyChanged();
            }
        }
        private DisplayItem[] displayItems = [];

        #region 展示类型定义

        public class DisplayItem : IMappingItem
        {
            public required string AssetType { get; set; }

            public required Type AssetClass { get; set; }

            public required Type WriteReadImplClass { get; set; }

            public required PageTypeMappingItem[] PageTypeMappingItems { get; set; }

        }

        public class PageTypeMappingItem(IPageTypeMappingItem item)
        {

            public Type AssetClass { get; } = item.AssetType;

            public string[] Tags { get; } = item.Tags ?? [];

            public string TagsStr { get => Tags.Length == 0 ? " < 无标签 > " : Common_Util.String.StringHelper.Concat(Tags, "; "); }

            public IPageTypeMappingItem MappingItem { get; } = item;
        }
        #endregion

    }
}
