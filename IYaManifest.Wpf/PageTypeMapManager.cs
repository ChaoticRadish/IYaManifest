using Common_Util.Enums;
using Common_Util.Extensions;
using IYaManifest.Attributes;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using static IYaManifest.Wpf.PageTypeMapManager;

namespace IYaManifest.Wpf
{
    /// <summary>
    /// 页面类型映射管理器
    /// </summary>
    public class PageTypeMapManager
    {
        #region 单例

        public static PageTypeMapManager Instance 
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new PageTypeMapManager();

                        }
                    }

                }
                return _instance;
            }
        }
        private static PageTypeMapManager? _instance;
        private static object _lock = new();
        private PageTypeMapManager() { }
        #endregion

        private Dictionary<Type, List<IPageTypeMappingItem>> MappingItems { get; set; } = [];

        public IEnumerable<IPageTypeMappingItem> this[Type assetType]
        {
            get
            {
                if (MappingItems.TryGetValue(assetType, out var item))
                {
                    return item!;
                }
                else
                {
                    throw new ArgumentException($"没有找到资源类型 {assetType} 对应的映射项");
                }
            }
        }

        /// <summary>
        /// 获取所有映射关系的字典
        /// </summary>
        public ReadOnlyDictionary<Type, IPageTypeMappingItem[]> All
        {
            get
            {
                return MappingItems.Select(i => KeyValuePair.Create(i.Key, i.Value.ToArray()))
                    .ToDictionary().AsReadOnly();
            }
        }
        /// <summary>
        /// 判断是否存在指定资源类型的映射关系
        /// </summary>
        /// <param name="assetType"></param>
        /// <returns></returns>
        public bool Exist(Type assetType)
        {
            return MappingItems.ContainsKey(assetType); 
        }

        #region 操作
        /// <summary>
        /// 尝试获取资源类型对应的页面类型映射项 (取第一个)
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryGet(Type assetType, [NotNullWhen(true)] out IPageTypeMappingItem? item)
        {
            if (MappingItems.TryGetValue(assetType, out var items))
            {
                item = items.FirstOrDefault();
                return item != null;
            }
            else
            {
                item = null;
                return false;
            }
        }
        /// <summary>
        /// 尝试获取资源类型对应的类型页面映射项 (所有映射项)
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public bool TryGetAll(Type assetType, [NotNullWhen(true)] out IEnumerable<IPageTypeMappingItem>? items)
        {
            if (MappingItems.TryGetValue(assetType, out var _items))
            {
                items = _items;
                return true;
            }
            else
            {
                items = null;
                return false;
            }
        }

        /// <summary>
        /// 尝试获取资源类型对应的页面类型映射项 (取第一个拥有指定标签的项)
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="tag"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryGetExistTag(Type assetType, string tag, [NotNullWhen(true)] out IPageTypeMappingItem? item)
        {
            if (MappingItems.TryGetValue(assetType, out var items))
            {
                item = items.FirstOrDefault(i => i.Tags.Contains(tag));
                return item != null;
            }
            else
            {
                item = null;
                return false;
            }
        }
        /// <summary>
        /// 尝试获取资源类型对应的页面类型映射项 (取第一个其拥有的标签满足指定条件的项)
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="where"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryGetWhereTag(Type assetType, Func<IEnumerable<string>, bool>? where, [NotNullWhen(true)] out IPageTypeMappingItem? item)
        {
            if (MappingItems.TryGetValue(assetType, out var items))
            {
                if (where == null)
                {
                    item = items.FirstOrDefault();
                }
                else
                {
                    item = items.FirstOrDefault(i => where(i.Tags));
                }
                return item != null;
            }
            else
            {
                item = null;
                return false;
            }
        }


        /// <summary>
        /// 尝试获取资源类型对应的页面类型映射项 (取第一个)
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool TryGet<TAsset>([NotNullWhen(true)] out IPageTypeMappingItem? item) where TAsset : IAsset
        {
            return TryGet(typeof(TAsset), out item);
        }

        /// <summary>
        /// 添加映射项
        /// </summary>
        /// <param name="item"></param>
        /// <param name="conflictDealMode">当准备加入的映射关系出现冲突时 (即已有相同类型相同标签的映射关系), 需要采取的处理方式</param>
        public void Add(IPageTypeMappingItem item, Common_Util.Enums.AppendConflictDealMode conflictDealMode = AppendConflictDealMode.Override)
        {
            ArgumentNullException.ThrowIfNull(item.AssetType);
            if (!MappingItems.TryGetValue(item.AssetType, out var items))
            {
                items = new List<IPageTypeMappingItem>();
                MappingItems[item.AssetType] = items;
            }
            var exist = items.FirstOrDefault(i => i.AssetType == item.AssetType && i.Tags.DisorderEquals(item.Tags));
            if (exist == null)
            {
                items.Add(item);
            }
            else
            {
                switch (conflictDealMode)
                {
                    case AppendConflictDealMode.Override:
                        int index = items.IndexOf(exist);
                        if (index < 0)
                        {
                            throw new InvalidOperationException("覆盖已有映射关系时, 未能取得已有映射关系在列表中的索引! ");
                        }
                        items[index] = item;
                        break;
                    case AppendConflictDealMode.Ignore:
                        return;
                    case AppendConflictDealMode.Exception:
                        throw new InvalidOperationException(
                            $"已有相同类型 ({item.AssetType.Name}) 相同标签 ({(item.Tags.Length == 0 ? "无标签": Common_Util.String.StringHelper.Concat(item.Tags, "; "))}) 的项");
                    default: throw new NotImplementedException($"未实现冲突处理方式: {conflictDealMode}");
                }
            }
        }

        /// <summary>
        /// 加载 DLL 文件, 寻找其中标记导出的映射项, 添加到当前映射管理器中
        /// </summary>
        /// <param name="dllPath"></param>
        /// <param name="conflictDealMode">加载过程中如果遇到冲突项 (即已有相同类型相同标签的映射关系), 需要采取的处理方式</param>
        public void AddFromDll(string dllPath, Common_Util.Enums.AppendConflictDealMode conflictDealMode = AppendConflictDealMode.Override)
        {
            List<IPageTypeMappingItem> waitAddItems = [];
            Assembly assembly = Assembly.LoadFrom(dllPath);
            Type[] types = assembly.GetExportedTypes();
            foreach (Type type in types)
            {
                if (!type.ExistCustomAttribute(out ExportMappingAttribute? attr))
                {
                    continue;
                }

                PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    if (!propertyInfo.PropertyType.IsAssignableTo(typeof(IPageTypeMappingItem)))
                    {
                        continue;
                    }
                    if (propertyInfo.ExistCustomAttribute<IgnoreMappingAttribute>())
                    {
                        continue;
                    }
                    object? value = propertyInfo.GetValue(null, null);
                    if (value is IPageTypeMappingItem item)
                    {
                        waitAddItems.Add(item);
                    }
                }

            }
            foreach (var item in waitAddItems)
            {
                Add(item);
            }
        }

        #endregion


        #region 实例化页面对象
        /// <summary>
        /// 找到特定类型的第一个所拥有的标签符合指定条件的映射项, 用其实例化一个创建器页面
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <returns>如果没有相应的映射项, 将返回 null</returns>
        public CreatorPage<TAsset>? InstanceCreator<TAsset>(Func<IEnumerable<string>, bool>? where = null)
            where TAsset : IAsset
        {
            IPageTypeMappingItem? item;
            if (!TryGetWhereTag(typeof(TAsset), where, out item))
            {
                return null;
            }
            if (item == null) { return null; }
            if (item.CreatorType == null) { return null; }
            return new(instanceCreatorFrom(item));
        }

        /// <summary>
        /// 找到特定类型的第一个所拥有的标签符合指定条件的映射项, 用其实例化一个显示器页面
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <returns>如果没有相应的映射项, 将返回 null</returns>
        public DisplayerPage<TAsset>? InstanceDisplayer<TAsset>(Func<IEnumerable<string>, bool>? where = null)
            where TAsset : IAsset
        {
            IPageTypeMappingItem? item;
            if (!TryGetWhereTag(typeof(TAsset), where, out item))
            {
                return null;
            }
            if (item == null) { return null; }
            if (item.DisplayerType == null) { return null; }
            return new(instanceDisplayerFrom(item));
        }

        /// <summary>
        /// 找到特定类型的第一个所拥有的标签符合指定条件的映射项, 用其实例化一个编辑器页面
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        /// <returns>如果没有相应的映射项, 将返回 null</returns>
        public EditorPage<TAsset>? InstanceEditor<TAsset>(Func<IEnumerable<string>, bool>? where = null)
            where TAsset : IAsset
        {
            IPageTypeMappingItem? item;
            if (!TryGetWhereTag(typeof(TAsset), where, out item))
            {
                return null;
            }
            if (item == null) { return null; }
            if (item.EditorType == null) { return null; }
            return new(instanceEditorFrom(item));
        }


        /// <summary>
        /// 通过特定映射项, 实例化创建器页面
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public CreatorPage InstanceCreatorFrom(IPageTypeMappingItem item)
        {
            return new CreatorPage(instanceCreatorFrom(item));
        }

        /// <summary>
        /// 通过特定映射项, 实例化显示器页面
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public DisplayerPage InstanceDisplayerFrom(IPageTypeMappingItem item)
        {
            return new DisplayerPage(instanceDisplayerFrom(item));
        }

        /// <summary>
        /// 通过特定映射项, 实例化编辑器器页面
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public EditorPage InstanceEditorFrom(IPageTypeMappingItem item)
        {
            return new EditorPage(instanceEditorFrom(item));
        }


        /// <summary>
        /// 自定义实例化创建器页面的方法
        /// </summary>
        public Func<IPageTypeMappingItem, object>? CustomInstanceCreatorPageFunc { get; set; }
        /// <summary>
        /// 自定义实例化显示器页面的方法
        /// </summary>
        public Func<IPageTypeMappingItem, object>? CustomInstanceDisplayerPageFunc { get; set; }
        /// <summary>
        /// 自定义实例化编辑器页面的方法
        /// </summary>
        public Func<IPageTypeMappingItem, object>? CustomInstanceEditorPageFunc { get; set; }


        private object instanceCreatorFrom(IPageTypeMappingItem item)
        {
            ArgumentNullException.ThrowIfNull(item.CreatorType);
            if (CustomInstanceCreatorPageFunc == null)
            {
                return instance(item.CreatorType);
            }
            else
            {
                return CustomInstanceCreatorPageFunc.Invoke(item);
            }
        }
        private object instanceDisplayerFrom(IPageTypeMappingItem item)
        {
            ArgumentNullException.ThrowIfNull(item.DisplayerType);
            if (CustomInstanceDisplayerPageFunc == null)
            {
                return instance(item.DisplayerType);
            }
            else
            {
                return CustomInstanceDisplayerPageFunc.Invoke(item);
            }
        }
        private object instanceEditorFrom(IPageTypeMappingItem item)
        {
            ArgumentNullException.ThrowIfNull(item.EditorType);
            if (CustomInstanceEditorPageFunc == null)
            {
                return instance(item.EditorType);
            }
            else
            {
                return CustomInstanceEditorPageFunc.Invoke(item);
            }
        }
        private object instance(Type type)
        {
            return Activator.CreateInstance(type) ?? throw new Exception($"调用 Activator.CreateInstance 实例化类型 {type} 返回 null");
        }

        #endregion
        public readonly struct CreatorPage : IAssetCreator
        {
            public CreatorPage(object obj)
            {
                ArgumentNullException.ThrowIfNull(obj);

                AsPage = (obj as Page) ?? throw new ArgumentException($"对象无法被转换为 {typeof(Page)}");
                AsCreator = (obj as IAssetCreator) ?? throw new ArgumentException($"对象无法被转换为 {typeof(IAssetCreator)}");
            }

            public Page AsPage { get; }

            public IAssetCreator AsCreator { get; }

            #region 通过 AsCreator 实现 IAssetCreator
            public IAsset? CreatedAsset => AsCreator.CreatedAsset;

            public event EventHandler<IAsset?> OnCreateDone
            {
                add
                {
                    AsCreator.OnCreateDone += value;
                }

                remove
                {
                    AsCreator.OnCreateDone -= value;
                }
            }
            #endregion
        }

        /// <summary>
        /// 特定资源类型的创建器页面
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        public readonly struct CreatorPage<TAsset> : IAssetCreator<TAsset>
            where TAsset : IAsset
        {
            public CreatorPage(object obj)
            {
                ArgumentNullException.ThrowIfNull(obj);

                AsPage = (obj as Page) ?? throw new ArgumentException($"对象无法被转换为 {typeof(Page)}");
                AsCreator = (obj as IAssetCreator<TAsset>) ?? throw new ArgumentException($"对象无法被转换为 {typeof(IAssetCreator<TAsset>)}");
            }

            public Page AsPage { get; }

            public IAssetCreator<TAsset> AsCreator { get; }

            #region 通过 AsCreator 实现 IAssetCreator<TAsset>
            public TAsset? CreatedAsset => AsCreator.CreatedAsset;

            IAsset? IAssetCreator.CreatedAsset => ((IAssetCreator)AsCreator).CreatedAsset;

            public event EventHandler<TAsset?> OnCreateDone
            {
                add
                {
                    AsCreator.OnCreateDone += value;
                }

                remove
                {
                    AsCreator.OnCreateDone -= value;
                }
            }

            event EventHandler<IAsset?> IAssetCreator.OnCreateDone
            {
                add
                {
                    ((IAssetCreator)AsCreator).OnCreateDone += value;
                }

                remove
                {
                    ((IAssetCreator)AsCreator).OnCreateDone -= value;
                }
            }
            #endregion
        }

        public readonly struct DisplayerPage : IAssetDisplayer
        {
            public DisplayerPage(object obj)
            {
                ArgumentNullException.ThrowIfNull(obj);

                AsPage = (obj as Page) ?? throw new ArgumentException($"对象无法被转换为 {typeof(Page)}");
                AsDisplayer = (obj as IAssetDisplayer) ?? throw new ArgumentException($"对象无法被转换为 {typeof(IAssetDisplayer)}");
            }

            public Page AsPage { get; }

            public IAssetDisplayer AsDisplayer { get; }

            #region 通过 AsDisplayer 实现 IAssetDisplayer
            public IAsset? Showing { get => AsDisplayer.Showing; set => AsDisplayer.Showing = value; }

            public event EventHandler<(IAsset? oldShowing, IAsset? newShowing)> OnShowingChanged
            {
                add
                {
                    AsDisplayer.OnShowingChanged += value;
                }

                remove
                {
                    AsDisplayer.OnShowingChanged -= value;
                }
            }

            public void Display()
            {
                AsDisplayer.Display();
            }
            #endregion
        }
        /// <summary>
        /// 特定资源类型的显示器页面
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        public readonly struct DisplayerPage<TAsset> : IAssetDisplayer<TAsset>
            where TAsset : IAsset
        {
            public DisplayerPage(object obj)
            {
                ArgumentNullException.ThrowIfNull(obj);

                AsPage = (obj as Page) ?? throw new ArgumentException($"对象无法被转换为 {typeof(Page)}");
                AsDisplayer = (obj as IAssetDisplayer<TAsset>) ?? throw new ArgumentException($"对象无法被转换为 {typeof(IAssetDisplayer<TAsset>)}");
            }

            public Page AsPage { get; }

            public IAssetDisplayer<TAsset> AsDisplayer { get; }

            #region 通过 AsDisplayer 实现 IAssetDisplayer<TAsset>
            public TAsset? Showing { get => AsDisplayer.Showing; set => AsDisplayer.Showing = value; }
            IAsset? IAssetDisplayer.Showing { get => ((IAssetDisplayer)AsDisplayer).Showing; set => ((IAssetDisplayer)AsDisplayer).Showing = value; }

            public event EventHandler<(TAsset? oldShowing, TAsset? newShowing)> OnShowingChanged
            {
                add
                {
                    AsDisplayer.OnShowingChanged += value;
                }

                remove
                {
                    AsDisplayer.OnShowingChanged -= value;
                }
            }

            event EventHandler<(IAsset? oldShowing, IAsset? newShowing)> IAssetDisplayer.OnShowingChanged
            {
                add
                {
                    ((IAssetDisplayer)AsDisplayer).OnShowingChanged += value;
                }

                remove
                {
                    ((IAssetDisplayer)AsDisplayer).OnShowingChanged -= value;
                }
            }

            public void Display()
            {
                AsDisplayer.Display();
            }

            #endregion
        }


        public readonly struct EditorPage : IAssetEditor
        {
            public EditorPage(object obj)
            {
                ArgumentNullException.ThrowIfNull(obj);

                AsPage = (obj as Page) ?? throw new ArgumentException($"对象无法被转换为 {typeof(Page)}");
                AsEditor = (obj as IAssetEditor) ?? throw new ArgumentException($"对象无法被转换为 {typeof(IAssetEditor)}");
            }

            public Page AsPage { get; }

            public IAssetEditor AsEditor { get; }


            #region 通过 AsEditor 实现 IAsEditor
            public IAsset? Input { get => AsEditor.Input; set => AsEditor.Input = value; }
            public IAsset? Output { get => AsEditor.Output; }

            public event EventHandler<IAsset> OnEditDone
            {
                add
                {
                    AsEditor.OnEditDone += value;
                }

                remove
                {
                    AsEditor.OnEditDone -= value;
                }
            }

            public void Reset()
            {
                AsEditor.Reset();
            }
            #endregion
        }


        /// <summary>
        /// 特定资源类型的显示器页面
        /// </summary>
        /// <typeparam name="TAsset"></typeparam>
        public readonly struct EditorPage<TAsset> : IAssetEditor<TAsset>
            where TAsset : IAsset
        {
            public EditorPage(object obj)
            {
                ArgumentNullException.ThrowIfNull(obj);

                AsPage = (obj as Page) ?? throw new ArgumentException($"对象无法被转换为 {typeof(Page)}");
                AsEditor = (obj as IAssetEditor<TAsset>) ?? throw new ArgumentException($"对象无法被转换为 {typeof(IAssetEditor<TAsset>)}");
            }

            public Page AsPage { get; }

            public IAssetEditor<TAsset> AsEditor { get; }

            #region 通过 AsEditor 实现 IAsEditor<TAsset>
            public TAsset? Input { get => AsEditor.Input; set => AsEditor.Input = value; }
            public TAsset? Output { get => AsEditor.Output; }
            IAsset? IAssetEditor.Input { get => (AsEditor).Input; set => (AsEditor).Input = value is TAsset ? default : (TAsset?)value; }
            IAsset? IAssetEditor.Output { get => (AsEditor).Output; }

            public event EventHandler<TAsset> OnEditDone
            {
                add
                {
                    AsEditor.OnEditDone += value;
                }

                remove
                {
                    AsEditor.OnEditDone -= value;
                }
            }

            event EventHandler<IAsset> IAssetEditor.OnEditDone
            {
                add
                {
                    ((IAssetEditor)AsEditor).OnEditDone += value;
                }

                remove
                {
                    ((IAssetEditor)AsEditor).OnEditDone -= value;
                }
            }

            public void Reset()
            {
                AsEditor.Reset();
            }

            #endregion
        }
    }

}
