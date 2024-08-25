using Common_Util.Interfaces.Behavior;
using IYaManifest.Interfaces;
using IYaManifest.Wpf.ViewModels;
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
using System.Windows.Shapes;

namespace IYaManifest.Wpf.Windows
{
    /// <summary>
    /// CommonAssetEditorWindow01.xaml 的交互逻辑
    /// </summary>
    public partial class CommonAssetEditorWindow01 : Window, IAssetEditor
    {
        #region 设置
        /// <summary>
        /// 资源编辑完成时, 是否关闭当前窗口
        /// </summary>
        public bool WhenDoneCloseWindow { get; set; } = true;
        #endregion

        public CommonAssetEditorWindow01()
        {
            InitializeComponent();
            ViewModel = new();
            ViewModel.OnEditDone += ViewModel_OnEditDone; ;
            ViewModel.OnCloseSignal += ViewModel_OnCloseSignal; ;
            this.DataContext = ViewModel;
        }


        private void ViewModel_OnCloseSignal(object? sender, bool? e)
        {
            DialogResult = e;
            Close();
        }

        private void ViewModel_OnEditDone(object? sender, IAsset e)
        {
            OnEditDone?.Invoke(sender, e);
            if (WhenDoneCloseWindow)
            {
                DialogResult = e != null;
                Close();
            }
        }


        private CommandAssetEditorWindow01ViewModel ViewModel;


        public IAsset? Input { get => ViewModel.Input; set => ViewModel.Input = value; }

        public IAsset? Output => ViewModel.Output;
        public void Reset()
        {
            ViewModel.Reset();
        }
        public event EventHandler<IAsset>? OnEditDone;



        /// <summary>
        /// 映射关系标签, 将查找拥有这个标签的映射关系
        /// </summary>
        public string? MappingTag { get => ViewModel.Tag; set => ViewModel.Tag = value; }
    }

    public class CommandAssetEditorWindow01ViewModel : CloseSignalViewModelBase<bool?>, IAssetEditor
    {
        private IAsset? input;
        public IAsset? Input 
        {
            get 
            {
                return input;
            }
            set
            {
                input = value;
                TargetAssetType = input?.GetType();
                if (Page != null)
                {
                    Page.Value.AsEditor.Input = input;
                }
            }
        }

        public IAsset? Output { get; private set; }

        public event EventHandler<IAsset>? OnEditDone;

        /// <summary>
        /// 触发事件 <see cref="OnEditDone"/>
        /// </summary>
        private void TriggerOnEditDone()
        {
            if (Output == null)
            {
                throw new ArgumentNullException(nameof(Output), "触发完成事件时, 编辑结果需要为非空值! ");
            }
            OnEditDone?.Invoke(this, Output);
        }

        public void Reset()
        {
            if (Page != null)
            {
                Page.Value.AsEditor.Reset();
                Output = Page.Value.Output;
            }
            else
            {
                Output = null;
            }

        }

        #region 页面映射关系

        private IPageTypeMappingItem? mapping;
        public IPageTypeMappingItem? Mapping
        {
            get => mapping;
            set
            {
                mapping = value;
                OnPropertyChanged();

                createPage();
            }
        }

        private void createPage()
        {
            if (Page != null)
            {
                if (Page.Value.AsPage is ICloseSignal<bool?> closeSignal)
                {
                    closeSignal.OnCloseSignal -= Page_OnCloseSignal;
                }

                Page.Value.AsEditor.OnEditDone -= Page_OnEditDone;
            }

            Page = null;
            if (Mapping == null) return;
            if (Mapping.EditorType == null) return;
            Page = PageTypeMapManager.Instance.InstanceEditorFrom(Mapping);

            if (Page.Value.AsPage is ICloseSignal<bool?> _closeSignal)
            {
                _closeSignal.OnCloseSignal += Page_OnCloseSignal;
            }

            Page.Value.AsEditor.OnEditDone += Page_OnEditDone;

        }

        private void Page_OnEditDone(object? sender, IAsset e)
        {
            if (sender is IAssetEditor editor)
            {
                Input = editor.Input;
                Output = editor.Output;
                TriggerOnEditDone();
            }
        }

        private void Page_OnCloseSignal(object? sender, bool? e)
        {
            if (e == true)
            {
                Output = Page?.Output;
            }
            TriggerCloseSignal(e, sender);
        }

        private PageTypeMapManager.EditorPage? page;
        public PageTypeMapManager.EditorPage? Page
        {
            get => page;
            set
            {
                page = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region 资源类
        private string? tag = null;
        public string? Tag
        {
            get => tag;
            set
            {
                tag = value;
                OnPropertyChanged();

                updateEditorPageType();
            }
        }


        private Type? targetAssetType;
        public Type? TargetAssetType
        {
            get => targetAssetType;
            private set
            {
                targetAssetType = value;
                OnPropertyChanged();

                updateEditorPageType();
            }
        }


        private bool gotMatchPageType;
        /// <summary>
        /// 是否取得匹配的页面类型
        /// </summary>
        public bool GotMatchPageType
        {
            get => gotMatchPageType;
            set
            {
                gotMatchPageType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NoGotMatchPageType));
            }
        }
        public bool NoGotMatchPageType => !GotMatchPageType;
        /// <summary>
        /// 未能获取到匹配的页面类型的相关描述
        /// </summary>
        public string NoGotInfo
        {
            get
            {
                StringBuilder sb = new();
                if (GotMatchPageType) sb.Append("已取得匹配的页面类型");
                else if (TargetAssetType == null) sb.Append("未设置资源类型");
                else if (Mapping == null)
                {
                    sb.Append("未能取得资源类型: ").Append(TargetAssetType);
                    if (Tag != null)
                    {
                        sb.Append('(').Append("映射标签: ").Append(Tag).Append(' ').Append(')');
                    }
                    sb.Append(' ').Append("对应的映射关系");
                }
                else if (Mapping.EditorType == null)
                {
                    sb.Append("资源类型: ").Append(TargetAssetType);
                    if (Tag != null)
                    {
                        sb.Append('(').Append("映射标签: ").Append(Tag).Append(' ').Append(')');
                    }
                    sb.Append(' ').Append("对应的映射关系未映射编辑页面类型");
                }
                return sb.ToString();
            }
        }




        private void updateEditorPageType()
        {
            GotMatchPageType = false;
            Mapping = null;

            if (TargetAssetType == null)
            {
                return;
            }
            else
            {
                IPageTypeMappingItem? item;
                if (Tag == null)
                {
                    if (!PageTypeMapManager.Instance.TryGet(TargetAssetType, out item))
                    {
                        return;
                    }
                }
                else
                {
                    if (!PageTypeMapManager.Instance.TryGetExistTag(TargetAssetType, Tag, out item))
                    {
                        return;
                    }
                }
                if (item != null)
                {
                    GotMatchPageType = item.EditorType != null;
                    Mapping = item;
                }

            }

        }

        #endregion
    }
}
