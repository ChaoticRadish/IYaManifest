using Common_Util.Interfaces.Behavior;
using IYaManifest.Interfaces;
using IYaManifest.Wpf.ViewModels;
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
    /// CommonAssetCreatorWindow01.xaml 的交互逻辑
    /// <para>通用的资源显示器窗口 01</para>
    /// <para>如果没有映射到页面类型, 则显示资源的基础信息, 且无法编辑 (永远不会创建完成)</para>
    /// </summary>
    public partial class CommonAssetCreatorWindow01 : Window, IAssetCreator
    {
        #region 设置
        /// <summary>
        /// 资源创建完成时, 是否关闭当前窗口
        /// </summary>
        public bool WhenDoneCloseWindow { get; set; } = true;
        #endregion

        public CommonAssetCreatorWindow01()
        {
            InitializeComponent();

            ViewModel = new();
            ViewModel.OnCreateDone += ViewModel_OnCreateDone;
            ViewModel.OnCloseSignal += ViewModel_OnCloseSignal;
            this.DataContext = ViewModel;
        }

        private void ViewModel_OnCloseSignal(object? sender, bool? e)
        {
            DialogResult = e;
            Close();
        }

        private void ViewModel_OnCreateDone(object? sender, IAsset? e)
        {
            OnCreateDone?.Invoke(sender, e);
            if (WhenDoneCloseWindow)
            {
                DialogResult = e != null;
                Close();
            }
        }

        private CommandAssetCreatorWindow01ViewModel ViewModel;

        public IAsset? CreatedAsset { get => ViewModel.CreatedAsset; }

        public event EventHandler<IAsset?>? OnCreateDone;

        /// <summary>
        /// 映射关系标签, 将查找拥有这个标签的映射关系
        /// </summary>
        public string? MappingTag { get => ViewModel.Tag; set => ViewModel.Tag = value; }
        /// <summary>
        /// 准备创建的资源类型
        /// </summary>
        public Type? TargetAssetType { get => ViewModel.TargetAssetType; set => ViewModel.TargetAssetType = value; }
    }

    public class CommandAssetCreatorWindow01ViewModel : CloseSignalViewModelBase<bool?>, IAssetCreator
    {
        public IAsset? CreatedAsset { get; private set; }

        public event EventHandler<IAsset?>? OnCreateDone;

        /// <summary>
        /// 触发事件 <see cref="OnCreateDone"/>
        /// </summary>
        private void TriggerOnCreateDone()
        {
            OnCreateDone?.Invoke(this, CreatedAsset);
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

                Page.Value.AsCreator.OnCreateDone -= Page_OnCreateDone;
            }


            Page = null;
            if (Mapping == null) return;
            if (Mapping.CreatorType == null) return;
            Page = PageTypeMapManager.Instance.InstanceCreatorFrom(Mapping);

            if (Page.Value.AsPage is ICloseSignal<bool?> _closeSignal)
            {
                _closeSignal.OnCloseSignal += Page_OnCloseSignal;
            }

            Page.Value.AsCreator.OnCreateDone += Page_OnCreateDone;

        }

        private void Page_OnCreateDone(object? sender, IAsset? e)
        {
            if (sender is IAssetCreator creator)
            {
                CreatedAsset = creator.CreatedAsset;
                TriggerOnCreateDone();
            }
        }

        private void Page_OnCloseSignal(object? sender, bool? e)
        {
            if (e == true)
            {
                CreatedAsset = Page?.CreatedAsset;
            }
            TriggerCloseSignal(e, sender);
        }

        private PageTypeMapManager.CreatorPage? page;
        public PageTypeMapManager.CreatorPage? Page
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

                updateCreatorPageType();
            }
        }


        private Type? targetAssetType;
        public Type? TargetAssetType 
        {
            get => targetAssetType;
            set
            {
                targetAssetType = value;
                OnPropertyChanged();

                updateCreatorPageType();
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
                else if (Mapping.CreatorType == null)
                {
                    sb.Append("资源类型: ").Append(TargetAssetType);
                    if (Tag != null)
                    {
                        sb.Append('(').Append("映射标签: ").Append(Tag).Append(' ').Append(')');
                    }
                    sb.Append(' ').Append("对应的映射关系未映射创建页面类型");
                }
                return sb.ToString();
            }
        }




        private void updateCreatorPageType()
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
                    GotMatchPageType = item.CreatorType != null;
                    Mapping = item;
                }

            }

        }

        #endregion
    }
}
