using IYaManifest.Interfaces;
using IYaManifestDemo.Assets;
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

namespace IYaManifestDemo.Pages.Assets
{
    /// <summary>
    /// ImageAssetDisplayerPage.xaml 的交互逻辑
    /// </summary>
    public partial class ImageAssetDisplayerPage : Page, IAssetDisplayer<ImageAsset>
    {
        public ImageAssetDisplayerPage()
        {
            InitializeComponent();

            ViewModel = new();

            DataContext = ViewModel;
        }
        private ImageAssetDisplayerPageViewModel ViewModel { get; set; }


        public ImageAsset? Showing 
        { 
            get => ((IAssetDisplayer<ImageAsset>)ViewModel).Showing; 
            set => ((IAssetDisplayer<ImageAsset>)ViewModel).Showing = value; 
        }
        
        IAsset? IAssetDisplayer.Showing { get => ((IAssetDisplayer)ViewModel).Showing; set => ((IAssetDisplayer)ViewModel).Showing = value; }

        public event EventHandler<(ImageAsset? oldShowing, ImageAsset? newShowing)> OnShowingChanged
        {
            add
            {
                ((IAssetDisplayer<ImageAsset>)ViewModel).OnShowingChanged += value;
            }

            remove
            {
                ((IAssetDisplayer<ImageAsset>)ViewModel).OnShowingChanged -= value;
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

        public void Display()
        {
            ((IAssetDisplayer)ViewModel).Display();
        }
    }

    public class ImageAssetDisplayerPageViewModel : AssetDisplayerBase<ImageAsset>, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        protected override void TriggerOnShowingChanged(ImageAsset? oldShowing, ImageAsset? newShowing)
        {
            base.TriggerOnShowingChanged(oldShowing, newShowing);
            OnPropertyChanged(nameof(Showing));
        }

        public override void Display()
        {
        }
    }
}
