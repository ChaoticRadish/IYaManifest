using Common_Util.Log;
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
    /// ManifestViewerPage.xaml 的交互逻辑
    /// </summary>
    public partial class ManifestViewerPage : Page
    {
        public ManifestViewerPage()
        {
            InitializeComponent();

            ViewModel.OperationLogger = LevelLoggerHelper.LogTo(LogShower, "操作", string.Empty);
            ViewModel.TrackLogger = LevelLoggerHelper.LogTo(LogShower, "执行过程", string.Empty);
            DataContext = ViewModel;
        }

        public ManifestViewerPageViewModel ViewModel { get; private set; } = new();
    }

}
