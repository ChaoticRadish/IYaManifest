using Common_Util.Enums;
using Common_Util.Interfaces.Owner;
using IYaManifest.Attributes;
using IYaManifest.Interfaces;
using IYaManifest.Wpf;
using IYaManifestAssetLibTest.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifestAssetLibTest
{
    [ExportMapping(AppendConflictDealMode.Exception)]
    public static class AssetMappingExport
    {
        public static MappingItem TestA => new()
        {
            AssetTypeEnum = AssetTypeEnum.TestA,
            AssetClass = typeof(AssetTestA),
            WriteReadImplClass = typeof(AssetTestAWRImpl),
            DisplayerType = typeof(AssetTestADisplayerPage),
            CreatorType = typeof(AssetTestACreatorPage),
            EditorType = typeof(AssetTestAEditorPage02),
        };
        public static MappingItem TestB => new()
        {
            AssetTypeEnum = AssetTypeEnum.TestB,
            AssetClass = typeof(AssetTestB),
            WriteReadImplClass = typeof(AssetTestBWRImpl),
            DisplayerType = typeof(AssetTestBDisplayerPage),
            CreatorType = typeof(AssetTestBCreatorPage),
            EditorType = null,
        };
    }
    public class MappingItem : IMappingItem, IPageTypeMappingItem
    {
        public required AssetTypeEnum AssetTypeEnum { get; set; }
        public string AssetType => AssetTypeEnum.ToString();

        public required Type AssetClass { get; set; }

        public required Type WriteReadImplClass { get; set; }

        public string[] Tags { get; set; } = [];
        IEnumerable<string?> ITagsOwner.Tags => Tags;

        public Type? CreatorType { get; set; }

        public Type? DisplayerType { get; set; }

        public Type? EditorType { get; set; }

        Type IPageTypeMappingItem.AssetType => AssetClass;

    }
}
