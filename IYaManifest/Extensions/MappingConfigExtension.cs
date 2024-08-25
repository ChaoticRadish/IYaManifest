using Common_Util.Enums;
using Common_Util.Extensions;
using IYaManifest.Attributes;
using IYaManifest.Core;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Extensions
{
    public static class MappingConfigExtension
    {
        /// <summary>
        /// 将一系列映射配置追加到配置中
        /// </summary>
        /// <param name="config"></param>
        /// <param name="conflictDealMode"></param>
        /// <param name="items"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(this MappingConfig config, 
            AppendConflictDealMode conflictDealMode, 
            IEnumerable<KeyValuePair<string, IMappingItem>> items)
        {
            foreach (var item in items)
            {
                config.Append(conflictDealMode, item.Key, item.Value);
            }
        }
        /// <summary>
        /// 将一系列映射配置追加到配置中
        /// </summary>
        /// <param name="config"></param>
        /// <param name="conflictDealMode"></param>
        /// <param name="items"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Append(this MappingConfig config,
            AppendConflictDealMode conflictDealMode,
            IEnumerable<IMappingItem> items)
        {
            foreach (var item in items)
            {
                config.Append(conflictDealMode, item);
            }
        }

        /// <summary>
        /// 加载动态链接库, 寻找其中标记输出的映射配置, 追加到配置中
        /// </summary>
        /// <param name="config"></param>
        /// <param name="conflictDealMode">追加冲突处理模式, 如果为 null, 则使用标记的建议处理模式</param>
        /// <param name="defaultConflictDealMode">默认的冲突处理模式, 如果传入的处理模式和标记的建议处理模式均为 null, 则改用此值</param>
        /// <param name="dllPath"></param>
        public static void AppendDll(this MappingConfig config,
            AppendConflictDealMode? conflictDealMode,
            AppendConflictDealMode defaultConflictDealMode,
            string dllPath)
        {

            List<(IMappingItem item, AppendConflictDealMode? suggestConflictDealMode) > items = new();

            Assembly assembly = Assembly.LoadFrom(dllPath);
            Type[] types = assembly.GetExportedTypes();
            foreach (Type type in types)
            {
                AppendConflictDealMode? suggestConflictDealMode = null;
                if (!type.ExistCustomAttribute(out ExportMappingAttribute? attr))
                {
                    suggestConflictDealMode = attr?.SuggestConflictDealMode;
                    continue;
                }

                PropertyInfo[] propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    if (!propertyInfo.PropertyType.IsAssignableTo(typeof(IMappingItem)))
                    {
                        continue;
                    }
                    if (propertyInfo.ExistCustomAttribute<IgnoreMappingAttribute>())
                    {
                        continue;
                    }
                    object? value = propertyInfo.GetValue(null, null);
                    if (value is IMappingItem item)
                    {
                        items.Add((item, suggestConflictDealMode));
                    }
                }

            }

            foreach ((IMappingItem item, AppendConflictDealMode? suggestConflictDealMode) in items)
            {
                AppendConflictDealMode dealMode = conflictDealMode ?? suggestConflictDealMode ?? defaultConflictDealMode;
                config.Append(dealMode, item);
            }
        }
    }
}
