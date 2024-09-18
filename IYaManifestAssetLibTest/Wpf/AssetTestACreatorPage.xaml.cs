using Common_Util.Data.Struct;
using Common_Util.Interfaces.Behavior;
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
    /// AssetTestACreatorPage.xaml 的交互逻辑
    /// </summary>
    public partial class AssetTestACreatorPage : Page, IAssetCreator<AssetTestA>, ICloseSignal<bool?>
    {
        public AssetTestACreatorPage()
        {
            InitializeComponent();

            ViewModel = new();
            DataContext = ViewModel;
        }

        private AssetTestACreatorPageViewModel ViewModel { get; set; }


        public AssetTestA? CreatedAsset => ((IAssetCreator<AssetTestA>)ViewModel).CreatedAsset;

        IAsset? IAssetCreator.CreatedAsset => ((IAssetCreator)ViewModel).CreatedAsset;

        public event EventHandler<AssetTestA?> OnCreateDone
        {
            add
            {
                ((IAssetCreator<AssetTestA>)ViewModel).OnCreateDone += value;
            }

            remove
            {
                ((IAssetCreator<AssetTestA>)ViewModel).OnCreateDone -= value;
            }
        }

        event EventHandler<IAsset?> IAssetCreator.OnCreateDone
        {
            add
            {
                ((IAssetCreator)ViewModel).OnCreateDone += value;
            }

            remove
            {
                ((IAssetCreator)ViewModel).OnCreateDone -= value;
            }
        }

        public event EventHandler<bool?>? OnCloseSignal
        {
            add
            {
                ((ICloseSignal<bool?>)ViewModel).OnCloseSignal += value;
            }

            remove
            {
                ((ICloseSignal<bool?>)ViewModel).OnCloseSignal -= value;
            }
        }

        event EventHandler<object?>? ICloseSignal.OnCloseSignal
        {
            add
            {
                ((ICloseSignal)ViewModel).OnCloseSignal += value;
            }

            remove
            {
                ((ICloseSignal)ViewModel).OnCloseSignal -= value;
            }
        }
    }

    public class AssetTestACreatorPageViewModel : AssetCreatorViewModelBase<AssetTestA>
    {
        private int valueIInput;
        public int ValueIInput
        {
            get => valueIInput;
            set
            {
                valueIInput = value;
                OnPropertyChanged();
            }
        }
        private string valueIStr = string.Empty;
        public string ValueIStr 
        {
            get => valueIStr;
            set
            {
                valueIStr = value;
                if (int.TryParse(valueIStr, out var i))
                {
                    ValueIInput = i;
                }
                OnPropertyChanged();
            } 
        }


        private float valueFInput;
        public float ValueFInput
        {
            get => valueFInput;
            set
            {
                valueFInput = value;
                OnPropertyChanged();
            }
        }
        private string valueFStr = string.Empty;
        public string ValueFStr
        {
            get => valueFStr;
            set
            {
                valueFStr = value;
                if (float.TryParse(valueFStr, out var f))
                {
                    ValueFInput = f;
                }
                OnPropertyChanged();
            }
        }

        protected override IOperationResult<AssetTestA> CreateImpl()
        {
            return (OperationResult<AssetTestA>)new AssetTestA()
            {
                Data = new()
                {
                    ValueF = ValueFInput,
                    ValueI = ValueIInput,
                }
            };
        }
    }
}
