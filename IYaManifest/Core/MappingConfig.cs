using Common_Util;
using Common_Util.Data.Struct;
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

        private readonly Dictionary<string, AnalysisSetting> AssetType2AnalysisDic = [];

        #endregion

        public AnalysisSetting this[string key] 
        { 
            get => AssetType2AnalysisDic[key]; 
        }

        public int Count => AssetType2AnalysisDic.Count;

        #region 操作
        public void Set(string assetType, AnalysisSetting setting)
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
        #endregion



        public class AnalysisSetting
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
