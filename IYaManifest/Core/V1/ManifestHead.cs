using Common_Util.Attributes.Xml;
using Common_Util.Data.Structure.Pair;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IYaManifest.Core.V1
{
    public class ManifestHead : IManifestHead
    {
        public const string DEFAULT_PACKAGE = "DEFAULT";

        [XmlIgnore]
        public GroupIdPair PackageKey { get; private set; }


        [XmlTextValue]
        public string Package
        { 
            get => packageStr;
            set
            {
                PackageKey = Common_Util.Data.Constraint.StringConveyingHelper.FromString<GroupIdPair>(value);
                packageStr = PackageKey.ToString();
            }
        }
        private string packageStr = string.Empty;

        [XmlTextValue]
        public string Version { get; set; } = string.Empty;

        [XmlTextValue]
        public string Name { get; set; } = string.Empty;

        [XmlTextValue]
        public string Remark { get; set; } = string.Empty;

        [XmlTextValue]
        public DateTime CreateTime { get; set; } = default;

        /// <summary>
        /// 对应清单文件的绝对路径, 不记录在清单文件中, 而是加载文件时赋值
        /// </summary>
        [XmlIgnore]
        public string? AbsolutePath { get;set; }
    }
}
