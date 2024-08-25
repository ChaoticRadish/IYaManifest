using IYaManifest.Interfaces;
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
using System.Windows.Shapes;

namespace IYaManifestDemo.Windows
{
    /// <summary>
    /// CommonDisplayerWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CommonDisplayerWindow : Window, IAssetDisplayer
    {
        public CommonDisplayerWindow()
        {
            InitializeComponent();
        }

        public IAsset? Showing { get => ((IAssetDisplayer)DisplayerPage).Showing; set => ((IAssetDisplayer)DisplayerPage).Showing = value; }

        public event EventHandler<(IAsset? oldShowing, IAsset? newShowing)> OnShowingChanged
        {
            add
            {
                ((IAssetDisplayer)DisplayerPage).OnShowingChanged += value;
            }

            remove
            {
                ((IAssetDisplayer)DisplayerPage).OnShowingChanged -= value;
            }
        }

        public void Display()
        {
            ((IAssetDisplayer)DisplayerPage).Display();
            Dispatcher.Invoke(Show);
        }
    }

}
