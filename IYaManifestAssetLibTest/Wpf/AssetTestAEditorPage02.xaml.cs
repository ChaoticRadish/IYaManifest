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
    /// AssetTestAEditorPage02.xaml 的交互逻辑
    /// </summary>
    public partial class AssetTestAEditorPage02 : Page, IAssetEditor<AssetTestA>, ICloseSignal<bool?>
    {
        public AssetTestAEditorPage02()
        {
            InitializeComponent();

            ViewModel = new();
            DataContext = ViewModel;
        }

        private AssetTestAEditorPageViewModel02 ViewModel { get; set; }

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
    public class AssetTestAEditorPageViewModel02 : AssetEditorViewModelBase_EditDone<AssetTestA>
    {
        ILevelLogger logger = Globals.DefaultLogger.Get("AssetLibTest", $"编辑资源 ({nameof(AssetTestA)}.v02)");

        public override void Reset()
        {
            base.Reset();
            ValueIStr = Input?.Data.ValueI.ToString() ?? string.Empty;
            ValueFStr = Input?.Data.ValueF.ToString() ?? string.Empty;

            logger.Info("Reset()");
        }

        protected override AssetTestA ConvertToOutput()
        {
            return new AssetTestA()
            {
                AssetId = Input?.AssetId ?? string.Empty,
                AssetType = Input?.AssetType ?? default,
                Data = new AssetTestA.DataPackage()
                {
                    ValueF = ValueFInput,
                    ValueI = ValueIInput,
                },
            };
        }


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
                if (int.TryParse(valueIStr, out var v))
                {
                    ValueIInput = v;
                    logger.Info($"设置 ValueIInput: {v}");
                }
                else
                {
                    ValueIInput = 0;
                    logger.Info($"ValueIInput 无效输入: {valueIStr}");
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
                if (float.TryParse(valueFStr, out var v))
                {
                    ValueFInput = v;
                    logger.Info($"设置 ValueFInput: {v}");
                }
                else
                {
                    ValueFInput = 0;
                    logger.Info($"ValueFInput 无效输入: {valueFStr}");
                }
                OnPropertyChanged();
            }
        }

    }
}
