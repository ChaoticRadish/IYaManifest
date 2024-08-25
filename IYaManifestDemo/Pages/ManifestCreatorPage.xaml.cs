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
    /// ManifestCreatorPage.xaml 的交互逻辑
    /// </summary>
    public partial class ManifestCreatorPage : Page
    {
        public ManifestCreatorPage()
        {
            InitializeComponent();

            ViewModel.OperationLogger = LevelLoggerHelper.LogTo(LogShower, "操作", string.Empty);
            ViewModel.TrackLogger = LevelLoggerHelper.LogTo(LogShower, "执行过程", string.Empty);
            ViewModel.UiInvoke = Dispatcher.Invoke;

            DataContext = ViewModel;
        }

        public ManifestCreatorPageViewModel ViewModel { get; set; } = new();

        private void TreeView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                // ListView拦截鼠标滚轮事件
                e.Handled = true;

                // 激发一个鼠标滚轮事件，冒泡给外层ListView接收到
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent?.RaiseEvent(eventArg);
            }
        }
    }
}
