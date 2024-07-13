using Common_Util.Data.Struct;
using Common_Util.Extensions;
using Common_Util.Module.Command;
using IYaManifest;
using IYaManifest.Core;
using IYaManifest.Core.Base;
using IYaManifest.Defines;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// TestManifestFile001.xaml 的交互逻辑
    /// </summary>
    public partial class TestManifestFile001 : Page
    {
        public TestManifestFile001()
        {
            InitializeComponent();

            ViewModel_Create = new();
            CreateText.DataContext = ViewModel_Create;

            ViewModel_ReadHead = new();
            ReadHead.DataContext = ViewModel_ReadHead;
        }

        public TestManifestFileViewModel_Create ViewModel_Create { get; private set; }

        public TestManifestFileViewModel_ReadHead ViewModel_ReadHead { get; private set; }
    }

    public class TestManifestFileViewModel_Create : TestViewModelBase
    {
        private string fileName = string.Empty;
        private string manifestDataInput = string.Empty;
        private string dataDataInput = string.Empty;

        public string FileName { get => fileName; set { fileName = value; OnPropertyChanged(); } }
        public string ManifestDataInput { get => manifestDataInput; set { manifestDataInput = value; OnPropertyChanged(); } }
        public string DataDataInput { get => dataDataInput; set { dataDataInput = value; OnPropertyChanged(); } }


        public ICommand TestCommand => new SampleCommand(_ => test(), _ => true);
        private void test()
        {
            Task.Run(async () =>
            {
                Globals.TestLogger?.Info("执行测试: 开始");
                try
                {
                    await testBody();
                    Globals.TestLogger?.Info("执行测试: 完成");
                }
                catch (Exception ex)
                {
                    Globals.TestLogger?.Error("执行测试: 异常", ex);
                }
            });
        }
        private async Task testBody()
        {
            using var manifestData = new MemoryStream(Encoding.UTF8.GetBytes(ManifestDataInput));
            using var dataData = new MemoryStream(Encoding.UTF8.GetBytes(DataDataInput));

            var creator = ManifestFileCreatorDefaultImpls.GetCreatorFromImpl(42,
                (version, appMark, dest) =>
                {
                    return ManifestFileCreatorDefaultImpls.FromStreamAsync(manifestData, dataData, version, appMark, dest);
                });
            await creator.CreateAsync(FileName);
        }

    }

    public class TestManifestFileViewModel_ReadHead : TestViewModelBase
    {
        private string fileName = string.Empty;
        private IOperationResultEx<ManifestFileHead> head = OperationResultEx<ManifestFileHead>.Failure("未读取");




        public string FileName 
        { 
            get => fileName; 
            set { fileName = value; OnPropertyChanged(); } 
        }

        public string HeadData
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (Head.IsSuccess)
                {
                    sb.AppendKeyValuePair("固定标记", Head.Data.Mark.ToHexString() + $" ({new string(Head.Data.Mark.Select(i => (char)i).ToArray())}) ");
                    sb.AppendKeyValuePair("清单区域起点", Head.Data.ManifestStart);
                    sb.AppendKeyValuePair("清单区域长度", Head.Data.ManifestLength);
                    sb.AppendKeyValuePair("清单区域MD5", Head.Data.ManifestMd5.ToHexString() + $"({Head.Data.ManifestMd5.Length})");
                    sb.AppendKeyValuePair("数据区域起点", Head.Data.DataStart);
                    sb.AppendKeyValuePair("数据区域长度", Head.Data.DataLength);
                    sb.AppendKeyValuePair("数据区域MD5", Head.Data.DataMd5.ToHexString() + $"({Head.Data.DataMd5.Length})");
                    sb.AppendKeyValuePair("文件格式版本号", Head.Data.Version);
                    sb.AppendKeyValuePair("关联应用的标记", Head.Data.AppMark);
                    sb.AppendKeyValuePair("CRC-8 校验码", Head.Data.CRC8);

                }

                return sb.ToString();
            }
        }

        public IOperationResultEx<ManifestFileHead> Head 
        {
            get => head;
            set
            {
                head = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HeadData));
            }
        } 

        public ICommand TestCommand => new SampleCommand(_ => test(), _ => true);
        private void test()
        {
            Task.Run(async () =>
            {
                Globals.TestLogger?.Info("执行测试: 开始");
                try
                {
                    await testBody();
                    Globals.TestLogger?.Info("执行测试: 完成");
                }
                catch (Exception ex)
                {
                    Globals.TestLogger?.Error("执行测试: 异常", ex);
                }
            });
        }

        private async Task testBody()
        {
            Head = await ManifestFileReadHelper.TryReadHeadAsync(FileName);
        }
    }
}
