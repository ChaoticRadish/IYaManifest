using Common_Util.Data.Struct;
using IYaManifest;
using IYaManifest.Core;
using IYaManifest.Core.Base;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IYaManifestDemo.TestPages
{
    /// <summary>
    /// TestManifestMapping001.xaml 的交互逻辑
    /// </summary>
    public partial class TestManifestMapping001 : Page
    {
        public TestManifestMapping001()
        {
            InitializeComponent();
        }


        private void test01()
        {
            MappingConfig config = new();
            Globals.TestLogger?.Info("01. 预期正常");
            try
            {
                config.Set("test01", new()
                {
                    AssetClass = typeof(TestAsset01),
                    WriteReadImplClass = typeof(TestAssetWriteReadImpl01),
                });
                Globals.TestLogger?.Info("01. 正常");
            }
            catch (Exception ex)
            {
                Globals.TestLogger?.Error("01. 异常!", ex);
            }
            Globals.TestLogger?.Info("02. 预期异常");
            try
            {
                config.Set("test02", new()
                {
                    AssetClass = typeof(TestAsset02),
                    WriteReadImplClass = typeof(TestAssetWriteReadImpl02),
                });
                Globals.TestLogger?.Info("02. 正常");
            }
            catch (Exception ex)
            {
                Globals.TestLogger?.Error("02. 异常!", ex);
            }
            Globals.TestLogger?.Info("03. 预期异常");
            try
            {
                config.Set("test03", new()
                {
                    AssetClass = typeof(TestAsset03),
                    WriteReadImplClass = typeof(TestAssetWriteReadImpl03),
                });
                Globals.TestLogger?.Info("03. 正常");
            }
            catch (Exception ex)
            {
                Globals.TestLogger?.Error("03. 异常!", ex);
            }
            Globals.TestLogger?.Info("04. 预期异常");
            try
            {
                config.Set("test04", new()
                {
                    AssetClass = typeof(TestAsset04),
                    WriteReadImplClass = typeof(TestAssetWriteReadImpl04),
                });
                Globals.TestLogger?.Info("04. 正常");
            }
            catch (Exception ex)
            {
                Globals.TestLogger?.Error("04. 异常!", ex);
            }
        }

        private void runTest(Action action, string actionInfo)
        {
            try
            {
                Globals.TestLogger?.Info($"[{actionInfo}] 执行测试: 开始");
                action();
                Globals.TestLogger?.Info($"[{actionInfo}] 执行测试: 结束");
            }
            catch (Exception ex)
            {
                Globals.TestLogger?.Error($"[{actionInfo}] 执行异常: " + ex.Message, ex);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            runTest(test01, "AnalysisSetting 初始化时类型检查");
        }

        private class TestAsset01 : IAsset
        {
            public string AssetType => "test01";

            public string AssetId { get; set; } = string.Empty;
        }
        private class TestAssetWriteReadImpl01 : AssetWriteReadImplBase<TestAsset01>
        {
            public override IOperationResult<TestAsset01> LoadFrom(Stream stream)
            {
                throw new NotImplementedException();
            }

            public override IOperationResult<TestAsset01> LoadFrom(byte[] data)
            {
                throw new NotImplementedException();
            }

            public override IOperationResult<byte[]> Serialization(TestAsset01 asset)
            {
                throw new NotImplementedException();
            }

            public override IOperationResult WriteTo(TestAsset01 asset, Stream stream)
            {
                throw new NotImplementedException();
            }
        }
        private class TestAsset02
        {
        }
        private class TestAssetWriteReadImpl02 : AssetWriteReadImplBase<TestAsset01>
        {
            public override IOperationResult<TestAsset01> LoadFrom(Stream stream)
            {
                throw new NotImplementedException();
            }

            public override IOperationResult<TestAsset01> LoadFrom(byte[] data)
            {
                throw new NotImplementedException();
            }

            public override IOperationResult<byte[]> Serialization(TestAsset01 asset)
            {
                throw new NotImplementedException();
            }

            public override IOperationResult WriteTo(TestAsset01 asset, Stream stream)
            {
                throw new NotImplementedException();
            }
        }
        private class TestAsset03 : IAsset
        {
            public string AssetType => "test03";

            public string AssetId { get; set; } = string.Empty;
        }
        private class TestAssetWriteReadImpl03
        {

        }
        private class TestAsset04 : IAsset
        {
            public string AssetType => "test04";

            public string AssetId { get; set; } = string.Empty;
        }
        private class TestAssetWriteReadImpl04 : AssetWriteReadImplBase<TestAsset03>
        {
            public override IOperationResult<TestAsset03> LoadFrom(Stream stream)
            {
                throw new NotImplementedException();
            }

            public override IOperationResult<TestAsset03> LoadFrom(byte[] data)
            {
                throw new NotImplementedException();
            }

            public override IOperationResult<byte[]> Serialization(TestAsset03 asset)
            {
                throw new NotImplementedException();
            }

            public override IOperationResult WriteTo(TestAsset03 asset, Stream stream)
            {
                throw new NotImplementedException();
            }
        }

    }
}
