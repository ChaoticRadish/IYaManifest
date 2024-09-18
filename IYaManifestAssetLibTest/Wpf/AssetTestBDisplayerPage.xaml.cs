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
    /// AssetTestBDisplayerPage.xaml 的交互逻辑
    /// </summary>
    public partial class AssetTestBDisplayerPage : Page, IAssetDisplayer<AssetTestB>
    {
        public AssetTestBDisplayerPage()
        {
            InitializeComponent();

            ViewModel = new();
            DataContext = ViewModel;
        }

        private AssetTestBDisplayerPageViewModel ViewModel { get; set; }

        public void Display() => ViewModel.Display();

        public AssetTestB? Showing { get => ((IAssetDisplayer<AssetTestB>)ViewModel).Showing; set => ((IAssetDisplayer<AssetTestB>)ViewModel).Showing = value; }

        IAsset? IAssetDisplayer.Showing { get => ((IAssetDisplayer)ViewModel).Showing; set => ((IAssetDisplayer)ViewModel).Showing = value; }

        public event EventHandler<(AssetTestB? oldShowing, AssetTestB? newShowing)> OnShowingChanged
        {
            add
            {
                ((IAssetDisplayer<AssetTestB>)ViewModel).OnShowingChanged += value;
            }

            remove
            {
                ((IAssetDisplayer<AssetTestB>)ViewModel).OnShowingChanged -= value;
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

    public class AssetTestBDisplayerPageViewModel : AssetDisplayerBase<AssetTestB>, INotifyPropertyChanged
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
