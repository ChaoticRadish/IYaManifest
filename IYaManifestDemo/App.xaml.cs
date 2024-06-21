using Common_Wpf.Themes;
using System.Configuration;
using System.Data;
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
        }
    }

}
