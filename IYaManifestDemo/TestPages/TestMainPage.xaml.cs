using Common_Util.Log;
using Common_Wpf.Controls.FeatureGroup;
using Common_Wpf.Extensions;
using IYaManifest;
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

namespace IYaManifestDemo.TestPages
{
    /// <summary>
    /// TestMainPage.xaml 的交互逻辑
    /// </summary>
    public partial class TestMainPage : Page
    {
        public TestMainPage()
        {
            InitializeComponent();


            Globals.TestLogger = LevelLoggerHelper.LogTo(LogShower);
            Globals.TestLogger.Info(" Globals.TestLogger 已设定! ");
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


        }

    }
}
