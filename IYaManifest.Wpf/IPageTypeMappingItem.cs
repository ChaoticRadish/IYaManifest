using Common_Util.Extensions;
using Common_Util.Interfaces.Owner;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace IYaManifest.Wpf
{
    public interface IPageTypeMappingItem : ITagsOwner
    {
        /// <summary>
        /// 资源类型
        /// </summary>
        public Type AssetType { get; }

        /// <summary>
        /// 页面信息标签, 当一个资源类型映射了多组页面时, 可以根据标签信息区分需要使用具体哪一个
        /// </summary>
        public new string[] Tags { get; }

        /// <summary>
        /// 创建器类型
        /// </summary>
        public Type? CreatorType { get; }

        /// <summary>
        /// 显示器类型
        /// </summary>
        public Type? DisplayerType { get; }

        /// <summary>
        /// 编辑器类型
        /// </summary>
        public Type? EditorType { get; }

    }

    public class PageTypeMappingItem : IPageTypeMappingItem
    {
        internal PageTypeMappingItem(Type assetType) { AssetType = assetType; }

        public string[] Tags { get; set; } = [];

        IEnumerable<string?> ITagsOwner.Tags => Tags;

        public Type AssetType { get; private set; }

        public Type? CreatorType { get; private set; }

        public Type? DisplayerType { get; private set; }

        public Type? EditorType { get; private set; }


        /// <summary>
        /// 创建页面类型映射项
        /// </summary>
        /// <param name="assetType">资源类型, 必须实现接口 <see cref="IYaManifest.Interfaces.IAsset"/></param>
        /// <param name="creatorType">创建器类型, 要求继承自类型 <see cref="System.Windows.Controls.Page"/>, 并实现接口 <see cref="IYaManifest.Interfaces.IAssetCreator{}"/></param>
        /// <param name="displayerType">显示器类型, 要求继承自类型 <see cref="System.Windows.Controls.Page"/>, 并实现接口 <see cref="IYaManifest.Interfaces.IAssetDisplayer{}"/></param>
        /// <param name="editorType">显示器类型, 要求继承自类型 <see cref="System.Windows.Controls.Page"/>, 并实现接口 <see cref="IYaManifest.Interfaces.IAssetEditor{}"/></param>
        /// <returns></returns>
        public static PageTypeMappingItem Create(
            Type assetType,
            Type? creatorType = null,
            Type? displayerType = null,
            Type? editorType = null)
        {
            PageTypeMappingItem item = new(assetType)
            {
                CreatorType = creatorType,
                DisplayerType = displayerType,
                EditorType = editorType,    
            };

            if (!assetType.IsAssignableTo(typeof(IAsset)))
            {
                throw new ArgumentException($"输入参数 {nameof(assetType)} 未实现 {typeof(IAsset)}");
            }
            if (creatorType != null)
            {
                if (!creatorType.IsAssignableTo(typeof(Page)))
                {
                    throw new ArgumentException($"输入参数 {nameof(creatorType)} 不继承自 {typeof(Page)}");
                }
                //if (!creatorType.HavePublicEmptyCtor())
                //{
                //    throw new ArgumentException($"输入参数 {nameof(creatorType)} 没有公共无参构造函数");
                //}
                Type 约束类型 = typeof(IAssetCreator<>).MakeGenericType(assetType);
                if (!creatorType.IsAssignableTo(约束类型))
                {
                    throw new ArgumentException($"输入参数 {nameof(creatorType)} 不继承自 {约束类型}");
                }
            }
            if (displayerType != null)
            {
                if (!displayerType.IsAssignableTo(typeof(Page)))
                {
                    throw new ArgumentException($"输入参数 {nameof(displayerType)} 不继承自 {typeof(Page)}");
                }
                //if (!displayerType.HavePublicEmptyCtor())
                //{
                //    throw new ArgumentException($"输入参数 {nameof(displayerType)} 没有公共无参构造函数");
                //}
                Type 约束类型 = typeof(IAssetDisplayer<>).MakeGenericType(assetType);
                if (!displayerType.IsAssignableTo(约束类型))
                {
                    throw new ArgumentException($"输入参数 {nameof(displayerType)} 不继承自 {约束类型}");
                }
            }

            if (editorType != null)
            {
                if (!editorType.IsAssignableTo(typeof(Page)))
                {
                    throw new ArgumentException($"输入参数 {nameof(editorType)} 不继承自 {typeof(Page)}");
                }
                //if (!editorType.HavePublicEmptyCtor())
                //{
                //    throw new ArgumentException($"输入参数 {nameof(editorType)} 没有公共无参构造函数");
                //}
                Type 约束类型 = typeof(IAssetEditor<>).MakeGenericType(assetType);
                if (!editorType.IsAssignableTo(约束类型))
                {
                    throw new ArgumentException($"输入参数 {nameof(editorType)} 不继承自 {约束类型}");
                }
            }

            return item;
        }
    }
}
