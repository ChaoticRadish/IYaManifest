using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Interfaces
{
    /// <summary>
    /// 资源创建器
    /// <para></para>
    /// </summary>
    public interface IAssetCreator
    {
        /// <summary>
        /// 被创建的对象
        /// </summary>
        public IAsset? CreatedAsset { get; }

        /// <summary>
        /// 创建完成事件
        /// </summary>
        public event EventHandler<IAsset?> OnCreateDone;
    }

    /// <summary>
    /// 专用于特定资源的资源创建器
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public interface IAssetCreator<TAsset> : IAssetCreator
        where TAsset : IAsset
    {
        /// <summary>
        /// 被创建的对象
        /// </summary>
        public new TAsset? CreatedAsset { get; }

        /// <summary>
        /// 创建完成事件
        /// </summary>
        public new event EventHandler<TAsset?> OnCreateDone;
    }

    /// <summary>
    /// 包含接口 <see cref="IAssetCreator{TAsset}"/> 默认实现的抽象基类
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public abstract class AssetCreatorBase<TAsset> : IAssetCreator<TAsset>
        where TAsset : IAsset
    {
        public abstract TAsset? CreatedAsset { get; protected set; }

        IAsset? IAssetCreator.CreatedAsset { get => CreatedAsset; }

        public event EventHandler<TAsset?>? OnCreateDone;

        private event EventHandler<IAsset?>? BaseInterfaceOnCreateDone;
        event EventHandler<IAsset?> IAssetCreator.OnCreateDone
        {
            add
            {
                BaseInterfaceOnCreateDone += value;
            }

            remove
            {
                BaseInterfaceOnCreateDone -= value;
            }
        }

        /// <summary>
        /// 触发创建完成事件
        /// </summary>
        protected virtual void TriggerOnCreateDone()
        {
            OnCreateDone?.Invoke(this, CreatedAsset);
            BaseInterfaceOnCreateDone?.Invoke(this, CreatedAsset);
        }
    }
}
