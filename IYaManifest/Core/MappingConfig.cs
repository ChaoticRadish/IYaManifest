using Common_Util;
using Common_Util.Data.Struct;
using Common_Util.Extensions;
using IYaManifest.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Core
{
    public class MappingConfig
    {
        #region 设置数据

        private readonly Dictionary<string, ConfigItem> AssetType2AnalysisDic = [];

        #endregion

        public ConfigItem this[string key] 
        { 
            get => AssetType2AnalysisDic[key]; 
        }

        public int Count => AssetType2AnalysisDic.Count;

        #region 操作
        /// <summary>
        /// 设置指定类型的配置项为传入值
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="setting"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Set(string assetType, ConfigItem setting)
        {
            var checkResult = setting.CheckImplMatch();
            if (!checkResult)
            {
                throw new InvalidOperationException("实现类型检查未通过, " + checkResult.FailureReason);
            }
            if (AssetType2AnalysisDic.TryGetValue(assetType, out _))
            {
                AssetType2AnalysisDic[assetType] = setting;
            }
            else
            {
                AssetType2AnalysisDic.Add(assetType, setting);
            }
        }
        /// <summary>
        /// 设置指定类型 (枚举值转换为字符串) 的配置项为传入值
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="enum"></param>
        /// <param name="setting"></param>
        /// <exception cref="ArgumentException"></exception>
        public void Set<TEnum>(TEnum @enum, ConfigItem setting) where TEnum : Enum
        {
            Set(@enum.ToString() ?? throw new ArgumentException("未能取得枚举值对应字符串"), setting);
        }

        /// <summary>
        /// 获取指定类型的配置项
        /// </summary>
        /// <param name="assetType"></param>
        /// <returns></returns>
        public ConfigItem? Get(string assetType)
        {
            if (AssetType2AnalysisDic.TryGetValue(assetType, out _))
            {
                return AssetType2AnalysisDic[assetType];
            }
            else { return null; }
        }
        /// <summary>
        /// 尝试获取指定类型的配置项
        /// </summary>
        /// <param name="assetType"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        public bool TryGet(string assetType, out ConfigItem? setting)
        {
            if (AssetType2AnalysisDic.TryGetValue(assetType, out _))
            {
                setting = AssetType2AnalysisDic[assetType];
                return true;
            }
            else
            {
                setting = null;
                return false;
            }
        }

        /// <summary>
        /// 获取指定类型 (枚举值转换为字符串) 的配置项
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="enum"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public ConfigItem? Get<TEnum>(TEnum @enum) where TEnum : Enum
        {
            return Get(@enum.ToString() ?? throw new ArgumentException("未能取得枚举值对应字符串"));
        }
        /// <summary>
        /// 尝试获取指定类型 (枚举值转换为字符串) 的配置项
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="enum"></param>
        /// <param name="setting"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public bool TryGet<TEnum>(TEnum @enum, out ConfigItem? setting) where TEnum : Enum
        {
            return TryGet(@enum.ToString() ?? throw new ArgumentException("未能取得枚举值对应字符串"), out setting);
        }
        #endregion



        public class ConfigItem
        {
            /// <summary>
            /// 资源数据实体类型
            /// </summary>
            public required Type AssetClass 
            { 
                get => assetClass!; 
                set
                {
                    if (value.IsAbstract)
                    {
                        throw new InvalidOperationException("类型是抽象类型");
                    }
                    if (!value.IsAssignableTo(typeof(IAsset)))
                    {
                        throw new InvalidOperationException($"类型不实现 {typeof(IAsset)} 接口");
                    }
                    assetClass = value;
                } 
            }
            private Type? assetClass;

            /// <summary>
            /// 实例化资源的方法, 如果为 null, 则将使用公共无参构造函数实例化
            /// </summary>
            public Func<object>? InstanceAssetFunc { get; set; }
            /// <summary>
            /// 实例化一个资源对象
            /// </summary>
            /// <returns></returns>
            public object InstanceAsset()
            {
                return instance(AssetClass, InstanceAssetFunc);
            }

            /// <summary>
            /// 读写实现类型
            /// </summary>
            public required Type WriteReadImplClass 
            {
                get => writeReadImplClass!; 
                set
                {
                    if (value.IsAbstract)
                    {
                        throw new InvalidOperationException("类型是抽象类型");
                    }
                    if (!TypeHelper.ExistInterfaceIsDefinitionFrom(value, typeof(IAssetWriteReadImpl<>)))
                    {
                        throw new InvalidOperationException($"类型不实现 {typeof(IAssetWriteReadImpl<>)} 接口");
                    }
                    writeReadImplClass = value;
                }
            }
            private Type? writeReadImplClass;

            /// <summary>
            /// 实例化读写实现的方法, 如果为 null, 则将使用公共无参构造函数实例化
            /// </summary>
            public Func<object>? InstanceWriteReadImplFunc { get; set; }
            /// <summary>
            /// 实例化一个读写实现对象
            /// </summary>
            /// <returns></returns>
            public object InstanceWriteReadImpl()
            {
                return instance(WriteReadImplClass, InstanceWriteReadImplFunc);
            }

            private object instance(Type type, Func<object>? instanceFunc)
            {
                if (instanceFunc == null)
                {
                    if (!type.HavePublicEmptyCtor())
                    {
                        throw new Exception("类型没有公共无参构造函数! ");
                    }
                    var output = Activator.CreateInstance(type)!;
                    if (output == null)
                    {
                        throw new Exception("实例化失败, 取得 null ");
                    }
                    return output;
                }
                else 
                {
                    var output = instanceFunc();
                    if (output == null)
                    {
                        throw new Exception("实例化失败, 实例化方法返回 null ");
                    }
                    if (output.GetType().IsAssignableTo(type))
                    {
                        throw new Exception("实例化取得对象无法被分配给指定类型"); 
                    }
                    return output;
                }
            }


            /// <summary>
            /// 检查读写实现类型与数据实体类型是否配套
            /// </summary>
            /// <returns></returns>
            public OperationResult CheckImplMatch()
            {
                try
                {
                    Type check = typeof(IAssetWriteReadImpl<>).MakeGenericType(AssetClass);
                    if (WriteReadImplClass.IsAssignableTo(check))
                    {
                        return true;
                    }
                    else
                    {
                        return $"不配套的资源类型, 实现类型({WriteReadImplClass})无法被赋值到 {check}";
                    }

                }
                catch (Exception ex) 
                {
                    Globals.MappingLogger?.Error($"检查映射类型是否配套发生异常, 资源({AssetClass}), 实现({WriteReadImplClass})", ex);
                    return $"检查异常: " + ex.Message;
                }

            }

        }
    }
}
