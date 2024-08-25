using Common_Util.Extensions;
using IYaManifest.Base.Interfaces;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IYaManifestDemo.Pages.Assets
{
    /// <summary>
    /// CommonDisplayerPage.xaml 的交互逻辑
    /// </summary>
    public partial class CommonDisplayerPage : Page, IAssetDisplayer
    {
        public CommonDisplayerPage()
        {
            InitializeComponent();

            DataContext = ViewModel;
        }

        private CommonDisplayerPageViewModel ViewModel { get; set; } = new();


        #region IAssetDisplayer
        public IAsset? Showing { get => ((IAssetDisplayer)ViewModel).Showing; set => ((IAssetDisplayer)ViewModel).Showing = value; }

        public event EventHandler<(IAsset? oldShowing, IAsset? newShowing)> OnShowingChanged
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
        #endregion
    }

    public class CommonDisplayerPageViewModel : ViewModelBase, IAssetDisplayer
    {
        private IAsset? showing;
        public IAsset? Showing { get => showing; set { showing = value; OnPropertyChanged(); onShowingChanged(); } }

        public event EventHandler<(IAsset? oldShowing, IAsset? newShowing)>? OnShowingChanged;

        private void onShowingChanged()
        {
            if (Showing == null)
            {
                ShowingText = "<null>";
            }
            else
            {
                ShowingText = convertToShowingText(Showing);
            }

        }

        private string showingText = string.Empty;
        public string ShowingText { get => showingText; set { showingText = value; OnPropertyChanged(); } }



        private string convertToShowingText(IAsset asset)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendKeyValuePair("资源类", asset.GetType());
            if (asset is IDataStringAsset dataStringAsset)
            {
                sb.AppendKeyValuePair("数据字符串", dataStringAsset.DataString, ": \n");
            }
            else
            {
                try
                {
                    sb.Append(asset.FullInfoString());
                }
                catch 
                {
                    sb.Append(asset.ToString());
                }
            }
            return sb.ToString();
        }


        public void Display() { }
    }
}
