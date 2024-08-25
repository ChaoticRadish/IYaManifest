using Common_Util.Module.Command;
using IYaManifest;
using IYaManifest.Core;
using IYaManifest.Extensions;
using IYaManifest.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// TestLoadDll001.xaml 的交互逻辑
    /// </summary>
    public partial class TestLoadDll001 : Page
    {
        public TestLoadDll001()
        {
            InitializeComponent();

            ViewModel = new();
            DataContext = ViewModel;
        }

        private TestLoadDll001ViewModel ViewModel { get; set; }
    }

    public class TestLoadDll001ViewModel : TestViewModelBase
    {
        public MappingConfig Config { get; set; } = new();

        public ICommand AppendDllCommand => new SampleCommand(_ => appendDll(), _ => true);

        private void appendDll()
        {
            try
            {

                OpenFileDialog dialog = new();
                if (dialog.ShowDialog() == true)
                {
                    Globals.TestLogger?.Info("加载 DLL 追加映射配置: " + dialog.FileName);
                    Config.AppendDll(
                        Common_Util.Enums.AppendConflictDealMode.Exception, 
                        Common_Util.Enums.AppendConflictDealMode.Exception, 
                        dialog.FileName);
                    Globals.TestLogger?.Info("加载 DLL 追加映射配置完成");
                }

            }
            catch (Exception ex)
            {
                Globals.TestLogger?.Error("加载 DLL 追加映射配置发生异常", ex);
            }
            finally
            {
                OnPropertyChanged(nameof(Config));
            }
        }

        public ReadOnlyDictionary<Type, IPageTypeMappingItem[]> AllPageTypeMappingItem => PageTypeMapManager.Instance.All;

        public ICommand AddPageTypeMappingFromDllCommand => new SampleCommand(_ => addPageTypeMappingFromDll(), _ => true);
        private void addPageTypeMappingFromDll()
        {
            try
            {

                OpenFileDialog dialog = new();
                if (dialog.ShowDialog() == true)
                {
                    Globals.TestLogger?.Info("加载 DLL 添加页面类型映射配置: " + dialog.FileName);
                    PageTypeMapManager.Instance.AddFromDll(dialog.FileName);
                    Globals.TestLogger?.Info("加载 DLL 添加页面类型配置完成");
                }

            }
            catch (Exception ex)
            {
                Globals.TestLogger?.Error("加载 DLL 添加页面类型映射配置发生异常", ex);
            }
            finally
            {
                OnPropertyChanged(nameof(AllPageTypeMappingItem));
            }
        }
    }
}
