using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Interfaces
{
    /// <summary>
    /// 资源显示器
    /// <para>可以获取或设定当前显示资源的显示器</para>
    /// </summary>
    public interface IAssetDisplayer
    {
        /// <summary>
        /// 当前显示的资源
        /// </summary>
        public IAsset? Showing { get; set; }

        /// <summary>
        /// 将显示器当前的状态转为显示
        /// </summary>
        /// <remarks>
        /// 部分资源显示器不一定常驻显示, 在设定完 <see cref="Showing"/> 后不一定处于显示状态 <br/>
        /// 例如: 窗口类型的显示器, 设置显示内容后, 还需要调用窗口的类似 Show() 的方法, 将窗口 <br/>
        /// 放置到前台展示, 这个时候就需要使用在此方法内实现
        /// </remarks>
        public void Display();

        /// <summary>
        /// 当前显示的资源变更事件
        /// </summary>
        public event EventHandler<(IAsset? oldShowing, IAsset? newShowing)> OnShowingChanged;
    }
    /// <summary>
    /// 专用于特定资源的资源创建器
    /// <para>可以获取或设定当前显示资源的显示器</para>
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public interface IAssetDisplayer<TAsset> : IAssetDisplayer
        where TAsset : IAsset
    {
        /// <summary>
        /// 当前显示的资源
        /// </summary>
        public new TAsset? Showing { get; set; }

        /// <summary>
        /// 当前显示的资源变更事件
        /// </summary>
        public new event EventHandler<(TAsset? oldShowing, TAsset? newShowing)> OnShowingChanged;
    }

    /// <summary>
    /// 包含接口 <see cref="IAssetDisplayer{TAsset}"/> 默认实现的抽象基类
    /// <para><see cref="TriggerOnShowingChanged"/> 方法会在设置 <see cref="Showing"/> 时被调用</para>
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public abstract class AssetDisplayerBase<TAsset> : IAssetDisplayer<TAsset>
        where TAsset : IAsset
    {
        public abstract void Display();

        public TAsset? Showing 
        {
            get => showing;
            set
            {
                var oldValue = showing;
                showing = value;
                TriggerOnShowingChanged(oldValue, showing);
            }
        }
        private TAsset? showing;
        IAsset? IAssetDisplayer.Showing { get => Showing; set => Showing = (TAsset?)value; }

        public event EventHandler<(TAsset? oldShowing, TAsset? newShowing)>? OnShowingChanged;

        private event EventHandler<(IAsset? oldShowing, IAsset? newShowing)>? BaseInterfaceOnShowingChanged;
        event EventHandler<(IAsset? oldShowing, IAsset? newShowing)> IAssetDisplayer.OnShowingChanged
        {
            add
            {
                BaseInterfaceOnShowingChanged += value;
            }

            remove
            {
                BaseInterfaceOnShowingChanged -= value;
            }
        }

        /// <summary>
        /// 触发当前显示资源变更事件
        /// </summary>
        /// <param name="oldShowing"></param>
        /// <param name="newShowing"></param>
        protected virtual void TriggerOnShowingChanged(TAsset? oldShowing, TAsset? newShowing)
        {
            OnShowingChanged?.Invoke(this, (oldShowing, newShowing));
            BaseInterfaceOnShowingChanged?.Invoke(this, (oldShowing, newShowing));
        }

    }
}
