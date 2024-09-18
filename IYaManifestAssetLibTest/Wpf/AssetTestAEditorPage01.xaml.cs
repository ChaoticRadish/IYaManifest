using Common_Util.Interfaces.Behavior;
using Common_Util.Log;
using IYaManifest;
using IYaManifest.Interfaces;
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

namespace IYaManifestAssetLibTest.Wpf
{
    /// <summary>
    /// AssetTestAEditorPage.xaml 的交互逻辑
    /// </summary>
    public partial class AssetTestAEditorPage01 : Page, IAssetEditor<AssetTestA>, ICloseSignal<bool?>
    {
        public AssetTestAEditorPage01()
        {
            InitializeComponent();


            ViewModel = new();
            DataContext = ViewModel;
        }

        private AssetTestAEditorPageViewModel01 ViewModel { get; set; }


        public AssetTestA? Input { get => ((IAssetEditor<AssetTestA>)ViewModel).Input; set => ((IAssetEditor<AssetTestA>)ViewModel).Input = value; }

        public AssetTestA? Output => ((IAssetEditor<AssetTestA>)ViewModel).Output;

        IAsset? IAssetEditor.Input { get => ((IAssetEditor)ViewModel).Input; set => ((IAssetEditor)ViewModel).Input = value; }

        IAsset? IAssetEditor.Output => ((IAssetEditor)ViewModel).Output;

        public event EventHandler<AssetTestA> OnEditDone
        {
            add
            {
                ((IAssetEditor<AssetTestA>)ViewModel).OnEditDone += value;
            }

            remove
            {
                ((IAssetEditor<AssetTestA>)ViewModel).OnEditDone -= value;
            }
        }

        event EventHandler<IAsset> IAssetEditor.OnEditDone
        {
            add
            {
                ((IAssetEditor)ViewModel).OnEditDone += value;
            }

            remove
            {
                ((IAssetEditor)ViewModel).OnEditDone -= value;
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

        public void Reset()
        {
            ((IAssetEditor)ViewModel).Reset();
        }
    }

    public class AssetTestAEditorPageViewModel01 : AssetEditorViewModelBase_Clone<AssetTestA>
    {
        ILevelLogger logger = Globals.DefaultLogger.Get("AssetLibTest", $"编辑资源 ({nameof(AssetTestA)}.v01)");
        protected override AssetTestA Clone(AssetTestA asset)
        {
            var output = new AssetTestA()
            {
                AssetId = asset.AssetId,
                AssetType = asset.AssetType,
                Data = asset.Data,
            };

            return output;
        }

        protected override void AfterInit()
        {
            ValueIStr = Input?.Data.ValueI.ToString() ?? string.Empty;
            ValueFStr = Input?.Data.ValueF.ToString() ?? string.Empty;
        }

        private string valueIStr = string.Empty;
        public string ValueIStr 
        {
            get => valueIStr;
            set
            {
                valueIStr = value;
                if (int.TryParse(valueIStr, out var v))
                {
                    ValueIInput = v;
                    logger.Info($"设置 ValueIInput: {v}");
                }
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
                if (float.TryParse(valueFStr, out var v))
                {
                    ValueFInput = v;
                    logger.Info($"设置 ValueFInput: {v}");
                }
                OnPropertyChanged();
            }
        }

        public int ValueIInput
        {
            get => Output?.Data.ValueI ?? 0;
            set
            {
                if (Output != null)
                {
                    Output.Data = new AssetTestA.DataPackage()
                    {
                        ValueI = value,
                        ValueF = Output.Data.ValueF,
                    };
                }
                OnPropertyChanged();
            }
        }

        public float ValueFInput
        {
            get => Output?.Data.ValueF ?? 0;
            set
            {
                if (Output != null)
                {
                    Output.Data = new AssetTestA.DataPackage()
                    {
                        ValueI = Output.Data.ValueI,
                        ValueF = value,
                    };
                }
                OnPropertyChanged();
            }
        }

    }
}
