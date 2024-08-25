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
using System.Windows.Shapes;

namespace IYaManifest.Wpf.Windows
{
    /// <summary>
    /// CommonAssetDisplayerWindow01.xaml 的交互逻辑
    /// <para>通用的资源显示器窗口 01</para>
    /// <para>如果没有映射到页面类型, 则显示资源的基础信息</para>
    /// </summary>
    public partial class CommonAssetDisplayerWindow01 : Window, IAssetDisplayer
    {
        public CommonAssetDisplayerWindow01()
        {
            InitializeComponent();

            ViewModel = new()
            {
                UiInvoke = Dispatcher.Invoke,
            };
            ViewModel.OnCloseSignal += ViewModel_OnCloseSignal;
            this.DataContext = ViewModel;
        }

        private void ViewModel_OnCloseSignal(object? sender, bool? e)
        {
            DialogResult = e;
            Close();
        }

        private CommonAssetDisplayerWindow01ViewModel ViewModel;

        /// <summary>
        /// 根据映射项的标签, 寻找符合条件的项的方法
        /// </summary>
        /// <remarks>
        /// 查找时会先按资源数据类获取到匹配的页面, 再按这个方法来查找符合条件的项 <br/>
        /// 如果这些符合条件的项有多个, 则使用 <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/> 取得其中的首个来使用
        /// </remarks>
        public Func<IEnumerable<string>, bool> FindPageFunc { get => ViewModel.FindPageFunc; set => ViewModel.FindPageFunc = value; }

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
            ViewModel.Display();
            Show();
        }
    }
    public class CommonAssetDisplayerWindow01ViewModel : INotifyPropertyChanged, IAssetDisplayer, ICloseSignal<bool?>
    {
        #region Ui线程
        internal Action<Action>? UiInvoke { get; init; }


        private void tryUiInvoke(Action action)
        {
            if (UiInvoke != null)
            {
                UiInvoke(action);
            }
            else
            {
                action();
            }
        }
        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region IAssetDisplayer

        public event EventHandler<(IAsset? oldShowing, IAsset? newShowing)>? OnShowingChanged;

        public void Display() 
        {
            Displayer?.Display();
        }

        private IAsset? showing;
        public IAsset? Showing 
        {
            get => showing; 
            set
            {
                var old = showing;
                showing = value;
                OnShowingChanged?.Invoke(this, (old, showing));
                OnPropertyChanged();
                showingChanged();
            } 
        }

        private void showingChanged()
        {
            if (showing == null)
            {
                PageTypeMappingItem = null;
            }
            else
            {
                Type assetType = showing.GetType();
                PageTypeMapManager.Instance.TryGetWhereTag(assetType, FindPageFunc, out var item);
                PageTypeMappingItem = item;
            }
        }
        #endregion

        #region 显示器页面
        public Func<IEnumerable<string>, bool> FindPageFunc { get; set; } = tags => true;

        public PageTypeMapManager.DisplayerPage? Displayer { get; private set; }

        private Page? displayerPage;
        public Page? DisplayerPage 
        { 
            get => displayerPage; 
            set 
            { 
                displayerPage = value; 
                OnPropertyChanged();
                displayerPageChanged();
            }
        }
        private void displayerPageChanged()
        {
            if (displayerPage != null)
            {
                if (displayerPage is ICloseSignal<bool?> closeSignal)
                {
                    closeSignal.OnCloseSignal += (o, e) =>
                    {
                        TriggerCloseSignal(e);
                    };
                }
            }
        }


        private string errorInfo = string.Empty;
        public string ErrorInfo { get => errorInfo; set { errorInfo = value; OnPropertyChanged(); } }


        private IPageTypeMappingItem? pageTypeMappingItem;
        public IPageTypeMappingItem? PageTypeMappingItem
        {
            get => pageTypeMappingItem;
            set 
            {
                pageTypeMappingItem = value;
                OnPropertyChanged();
                pageTypeMappingItemChanged();
            }
        }
        private void pageTypeMappingItemChanged()
        {
            Displayer = null;
            ErrorInfo = string.Empty;

            if (pageTypeMappingItem == null)
            {
                DisplayerPage = null;
            }
            else
            {
                try
                {
                    Displayer = PageTypeMapManager.Instance.InstanceDisplayerFrom(pageTypeMappingItem);
                    Displayer.Value.AsDisplayer.Showing = showing;
                    DisplayerPage = Displayer.Value.AsPage;
                }
                catch (Exception ex)
                {
                    ErrorInfo = ex.ToString();
                }
            }

        }

        #endregion

        #region 页面关闭信号
        public event EventHandler<bool?>? OnCloseSignal;

        private event EventHandler<object?>? baseInterfaceOnCloseSignal;

        event EventHandler<object?>? ICloseSignal.OnCloseSignal
        {
            add
            {
                baseInterfaceOnCloseSignal += value;
            }

            remove
            {
                baseInterfaceOnCloseSignal -= value;
            }
        }

        /// <summary>
        /// 触发关闭信号事件
        /// </summary>
        /// <param name="arg"></param>
        protected void TriggerCloseSignal(bool? arg, object? sender = null)
        {
            OnCloseSignal?.Invoke(sender ?? this, arg);
            baseInterfaceOnCloseSignal?.Invoke(sender ?? this, arg);
        }

        #endregion
    }
}
