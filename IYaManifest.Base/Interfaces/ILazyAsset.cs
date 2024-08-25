using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Interfaces
{
    /// <summary>
    /// 懒加载资源接口
    /// </summary>
    public interface ILazyAsset : IAsset
    {
        /// <summary>
        /// 以懒加载模式管理的资源
        /// </summary>
        public IAsset? Asset { get; }

        /// <summary>
        /// 加载资源
        /// </summary>
        public void Load();
        /// <summary>
        /// 卸载已加载的资源
        /// </summary>
        public void Unload();


        /// <summary>
        /// 资源是否已经载入
        /// </summary>
        public bool Loaded { get; }

        /// <summary>
        /// 预期引用类型, 即加载完成后, <see cref="Asset"/> 对象的类型
        /// </summary>
        public Type? ExpectReferenceType { get; }

        /// <summary>
        /// 是否已载入状态变更事件
        /// </summary>
        public event EventHandler LoadedStateChanged;
    }
    public interface ILazyAsset<TAsset> : ILazyAsset
        where TAsset : IAsset
    {
        /// <summary>
        /// 以懒加载模式管理的资源
        /// </summary>
        public new TAsset? Asset { get; }
    }
}
