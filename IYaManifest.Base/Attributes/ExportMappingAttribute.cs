using Common_Util.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Attributes
{
    /// <summary>
    /// 标记类型中, 类型属于 <see cref="Interfaces.IMappingItem"/> 的公共静态属性为需要导出映射关系的项
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportMappingAttribute : Attribute
    {
        public ExportMappingAttribute()
        {
            SuggestConflictDealMode = null;
        }
        public ExportMappingAttribute(AppendConflictDealMode suggestConflictDealMode)
        {
            SuggestConflictDealMode = suggestConflictDealMode;
        }


        /// <summary>
        /// 如果发生冲突, 建议的处理方式
        /// </summary>
        public AppendConflictDealMode? SuggestConflictDealMode { get; private set; }
    }

    /// <summary>
    /// 标记一个类型属于 <see cref="Interfaces.IMappingItem"/> 的公共静态属性, 在导出映射关系时, 需要被忽略
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreMappingAttribute : Attribute { }
}
