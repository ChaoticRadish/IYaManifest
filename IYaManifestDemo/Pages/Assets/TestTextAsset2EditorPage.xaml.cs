using Common_Util.Interfaces.Behavior;
using Common_Util.Module.Command;
using IYaManifest.Interfaces;
using IYaManifestDemo.Assets;
using IYaManifestDemo.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    /// TestTextAsset2EditorPage.xaml 的交互逻辑
    /// </summary>
    public partial class TestTextAsset2EditorPage : Page, IAssetEditor<TestTextAsset2>, ICloseSignal<bool?>
    {
        public TestTextAsset2EditorPage()
        {
            InitializeComponent();

            ViewModel = new();
            DataContext = ViewModel;

        }

        public TestTextAsset2EditorPageViewModel ViewModel { get; private set; }


        public TestTextAsset2? Input { get => ((IAssetEditor<TestTextAsset2>)ViewModel).Input; set => ((IAssetEditor<TestTextAsset2>)ViewModel).Input = value; }

        public TestTextAsset2? Output => ((IAssetEditor<TestTextAsset2>)ViewModel).Output;

        IAsset? IAssetEditor.Input { get => ((IAssetEditor)ViewModel).Input; set => ((IAssetEditor)ViewModel).Input = value; }

        IAsset? IAssetEditor.Output => ((IAssetEditor)ViewModel).Output;

        public event EventHandler<TestTextAsset2> OnEditDone
        {
            add
            {
                ((IAssetEditor<TestTextAsset2>)ViewModel).OnEditDone += value;
            }

            remove
            {
                ((IAssetEditor<TestTextAsset2>)ViewModel).OnEditDone -= value;
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


        public void Reset()
        {
            ((IAssetEditor)ViewModel).Reset();
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

    public class TestTextAsset2EditorPageViewModel : EditorViewModelBase<TestTextAsset2>
    {
        

        #region 属性
        public string TextInput
        {
            get => Editing?.Data ?? string.Empty;
            set
            {
                if (Editing != null)
                {
                    Editing.Data = value;
                }
            }
        }
        #endregion


        protected override TestTextAsset2 Clone(TestTextAsset2 asset)
        {
            return new TestTextAsset2()
            {
                Data = asset.Data,
                AssetId = asset.AssetId,
            };
        }
    }
}
