using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace IYaManifestAssetLibTest.Wpf
{
    /// <summary>
    /// AssetTestADisplayerPage.xaml 的交互逻辑
    /// </summary>
    public partial class AssetTestADisplayerPage : Page, IAssetDisplayer<AssetTestA>
    {
        public AssetTestADisplayerPage()
        {
            InitializeComponent();

            ViewModel = new();
            DataContext = ViewModel;
        }

        private AssetTestADisplayerPageViewModel ViewModel { get; set; }

        public void Display() => ViewModel.Display();


        public AssetTestA? Showing { get => ((IAssetDisplayer<AssetTestA>)ViewModel).Showing; set => ((IAssetDisplayer<AssetTestA>)ViewModel).Showing = value; }
        
        IAsset? IAssetDisplayer.Showing { get => ((IAssetDisplayer)ViewModel).Showing; set => ((IAssetDisplayer)ViewModel).Showing = value; }

        public event EventHandler<(AssetTestA? oldShowing, AssetTestA? newShowing)> OnShowingChanged
        {
            add
            {
                ((IAssetDisplayer<AssetTestA>)ViewModel).OnShowingChanged += value;
            }

            remove
            {
                ((IAssetDisplayer<AssetTestA>)ViewModel).OnShowingChanged -= value;
            }
        }

        event EventHandler<(IAsset? oldShowing, IAsset? newShowing)> IAssetDisplayer.OnShowingChanged
        {
            add
            {
                ((IAssetDisplayer)ViewModel).OnShowingChanged += value;
            }

            remove
            {
                ((IAssetDisplayer)ViewModel).OnShowingChanged -= value;
            }
        }

    }

    public class AssetTestADisplayerPageViewModel : AssetDisplayerBase<AssetTestA>, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public override void Display()
        {
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }




    }
}
