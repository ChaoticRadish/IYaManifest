using Common_Util.Data.Exceptions;
using Common_Util.Data.Struct;
using Common_Util.Data.Structure.Value;
using IYaManifest.Core.Base;
using IYaManifest.Enums;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Core.V1
{
    /// <summary>
    /// 资源加载方法的选择器, 选择后根据设定, 取得模式对应的处理器处理数据
    /// </summary>
    public class AssetLoadModeSelector
    {
        public required AssetWriteReader AssetWriteReader;

        /// <summary>
        /// 选择加载方法的委托
        /// </summary>
        /// <param name="item"></param>
        /// <param name="length">数据长度</param>
        /// <returns>当加载模式是</returns>
        public delegate (AssetLoadModeEnum mode, string customKey) SelectFuncDelegate(ManifestItem item, long length);

        /// <summary>
        /// 选择实现方式的具体方法
        /// </summary>
        public required SelectFuncDelegate SelectFunc { get; set; }



        /// <summary>
        /// 此方法需要在读取到清单头之后, 读取清单项之前调用
        /// </summary>
        /// <param name="head"></param>
        public void BeforeReadItems(ManifestHead head)
        {
            ergodicHandlers(item => item.BeforeReadItems(head));
        }
        /// <summary>
        /// 此方法需要在读取清单完成之后调用
        /// </summary>
        /// <param name="items"></param>
        public void AfterReadItems(ManifestItem[] items)
        {
            ergodicHandlers(item => item.AfterReadItems(items));
        }
        /// <summary>
        /// 读取清单文件过程中出现失败的情况
        /// </summary>
        /// <param name="result"></param>
        public void ManifestReadFailure(IOperationResult result)
        {
            ergodicHandlers(item => item.ManifestReadFailure(result));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ergodicHandlers(Action<CustomAssetLoadHandler> action)
        {
            action.Invoke(ToObjectHandler);
            action.Invoke(LazyAssetHandler);
            foreach (var item in customHandlers.Values)
            {
                action.Invoke(item);
            }
        }

        /// <summary>
        /// 处理资源项
        /// </summary>
        /// <param name="item"></param>
        /// <param name="stream"></param>
        /// <param name="length"></param>
        public void Handle(ManifestItem item, Stream stream, long length)
        {
            (AssetLoadModeEnum mode, string customKey) = SelectFunc(item, length);
            IAsset? asset = null;
            switch (mode)
            {
                case AssetLoadModeEnum.ToObject:
                    asset = ToObjectHandler.Handle(item, stream, length, AssetWriteReader);
                    break;
                case AssetLoadModeEnum.LazyAsset:
                    asset = LazyAssetHandler.Handle(item, stream, length, AssetWriteReader);
                    break;
                case AssetLoadModeEnum.Custom:
                    if (customHandlers.TryGetValue(customKey, out var handler))
                    {
                        asset = handler.Handle(item, stream, length, AssetWriteReader);
                    }
                    break;
            }
            if (asset == null)
            {
                throw new OperationFailureException((OperationResult)$"资源项 {item.AssetId} 未取得对应资源对象引用");
            }
            item.AssetReference = asset;
        }

        #region 常规的处理方法
        public CustomAssetLoadHandler ToObjectHandler { get; set; } = DefaultToObjectLoadHanlder.Instance;

        public CustomAssetLoadHandler LazyAssetHandler { get; set; } = DefaultOutsideLazyFileAssetLoadHandler.Instance;
        #endregion

        #region 自定义处理的设定
        private Dictionary<string, CustomAssetLoadHandler> customHandlers = [];
        /// <summary>
        /// 添加自定义处理设置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="handler"></param>
        public void AddCustom(string key, CustomAssetLoadHandler handler)

        {
            if (!customHandlers.TryAdd(key, handler))
            {
                customHandlers[key] = handler;
            }
        }
        /// <summary>
        /// 移除自定义处理设置
        /// </summary>
        /// <param name="key"></param>
        public void RemoveCustom(string key)
        {
            customHandlers.Remove(key);
        }
        
        #endregion

        #region 静态的常用选择方法
        /// <summary>
        /// 无论什么样子的清单项, 均直接转换为对象赋值到 <see cref="ManifestItem.AssetReference"/>
        /// </summary>
        public static SelectFuncDelegate AllToObject { get; } = (item, length) => (AssetLoadModeEnum.ToObject, string.Empty);


        #endregion
    }
}
