using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Attributes
{
    /// <summary>
    /// 标记一个属性是资源的头信息属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class HeadInfoAttribute : Attribute
    {
    }
}
