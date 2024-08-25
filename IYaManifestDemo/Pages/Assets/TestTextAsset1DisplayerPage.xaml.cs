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
    /// TestTextAsset1DisplayerPage.xaml 的交互逻辑
    /// </summary>
    public partial class TestTextAsset1DisplayerPage : Page, IAssetDisplayer<TestTextAsset1>
    {
        public TestTextAsset1DisplayerPage()
        {
            InitializeComponent();

            ViewModel = new();

            DataContext = ViewModel;
        }
        private TestTextAsset1DisplayerPageViewModel ViewModel { get; set; }


        public TestTextAsset1? Showing { get => ((IAssetDisplayer<TestTextAsset1>)ViewModel).Showing; set => ((IAssetDisplayer<TestTextAsset1>)ViewModel).Showing = value; }

        IAsset? IAssetDisplayer.Showing { get => ((IAssetDisplayer)ViewModel).Showing; set => ((IAssetDisplayer)ViewModel).Showing = value; }

        public event EventHandler<(TestTextAsset1? oldShowing, TestTextAsset1? newShowing)> OnShowingChanged
        {
            add
            {
                ((IAssetDisplayer<TestTextAsset1>)ViewModel).OnShowingChanged += value;
            }

            remove
            {
                ((IAssetDisplayer<TestTextAsset1>)ViewModel).OnShowingChanged -= value;
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


    public class TestTextAsset1DisplayerPageViewModel : AssetDisplayerBase<TestTextAsset1>, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        protected override void TriggerOnShowingChanged(TestTextAsset1? oldShowing, TestTextAsset1? newShowing)
        {
            base.TriggerOnShowingChanged(oldShowing, newShowing);
            OnPropertyChanged(nameof(Showing));
        }

        public override void Display()
        {
        }
    }
}
