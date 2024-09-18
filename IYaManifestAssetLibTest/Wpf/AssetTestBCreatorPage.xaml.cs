using Common_Util.Data.Struct;
using Common_Util.Extensions;
using Common_Util.Interfaces.Behavior;
using IYaManifest;
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
    /// AssetTestBCreatorPage.xaml 的交互逻辑
    /// </summary>
    public partial class AssetTestBCreatorPage : Page, IAssetCreator<AssetTestB>, ICloseSignal<bool?>
    {
        public AssetTestBCreatorPage()
        {
            InitializeComponent();

            ViewModel = new();
            DataContext = ViewModel;
        }

        private AssetTestBCreatorPageViewModel ViewModel { get; set; }


        public AssetTestB? CreatedAsset => ((IAssetCreator<AssetTestB>)ViewModel).CreatedAsset;

        IAsset? IAssetCreator.CreatedAsset => ((IAssetCreator)ViewModel).CreatedAsset;

        public event EventHandler<AssetTestB?> OnCreateDone
        {
            add
            {
                ((IAssetCreator<AssetTestB>)ViewModel).OnCreateDone += value;
            }

            remove
            {
                ((IAssetCreator<AssetTestB>)ViewModel).OnCreateDone -= value;
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

    public class AssetTestBCreatorPageViewModel : AssetCreatorViewModelBase<AssetTestB>
    {
        private byte[] dataInput = [];
        public byte[] DataInput
        {
            get => dataInput; 
            set 
            { 
                dataInput = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentData));
            }
        }


        public string CurrentData { get => dataInput.Length == 0 ? "Empty" : dataInput.ToHexString(); }

        protected override IOperationResult<AssetTestB> CreateImpl()
        {
            var logger = Globals.DefaultLogger.Get("AssetTestB 资源创建页", "创建资源实例");

            byte[] copy = new byte[DataInput.Length];
            DataInput.CopyTo(copy.AsMemory());

            logger.Info($"copy => {(copy.Length == 0 ? "Empty" : copy.ToHexString())}");

            logger.Info($"DataInput => {(DataInput.Length == 0 ? "Empty" : DataInput.ToHexString())}");

            return (OperationResult<AssetTestB>)new AssetTestB()
            {
                Data = copy,
            };
        }
    }
}
