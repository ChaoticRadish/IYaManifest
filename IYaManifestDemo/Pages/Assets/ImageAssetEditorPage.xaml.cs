using Common_Util.Data.Exceptions;
using Common_Util.Data.Struct;
using Common_Util.Extensions;
using Common_Util.Interfaces.Behavior;
using Common_Util.Log;
using Common_Util.Module.Command;
using IYaManifest.Interfaces;
using IYaManifestDemo.Assets;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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

namespace IYaManifestDemo.Pages.Assets
{
    /// <summary>
    /// ImageAssetEditorPage.xaml 的交互逻辑
    /// </summary>
    public partial class ImageAssetEditorPage : Page, IAssetEditor<ImageAsset>, ICloseSignal<bool?>
    {
        public ImageAssetEditorPage()
        {
            InitializeComponent();

            ViewModel = new();
            DataContext = ViewModel;
        }

        private ImageAssetEditorPageViewModel ViewModel;


        public event EventHandler<ImageAsset> OnEditDone
        {
            add
            {
                ((IAssetEditor<ImageAsset>)ViewModel).OnEditDone += value;
            }

            remove
            {
                ((IAssetEditor<ImageAsset>)ViewModel).OnEditDone -= value;
            }
        }

        event EventHandler<IAsset> IAssetEditor.OnEditDone
        {
            add
            {
                ((IAssetEditor)ViewModel).OnEditDone += value;
            }

            remove
            {
                ((IAssetEditor)ViewModel).OnEditDone -= value;
            }
        }

        public event EventHandler<bool?>? OnCloseSignal
        {
            add
            {
                ((ICloseSignal<bool?>)ViewModel).OnCloseSignal += value;
            }

            remove
            {
                ((ICloseSignal<bool?>)ViewModel).OnCloseSignal -= value;
            }
        }

        event EventHandler<object?>? ICloseSignal.OnCloseSignal
        {
            add
            {
                ((ICloseSignal)ViewModel).OnCloseSignal += value;
            }

            remove
            {
                ((ICloseSignal)ViewModel).OnCloseSignal -= value;
            }
        }

        public ImageAsset? Input { get => ((IAssetEditor<ImageAsset>)ViewModel).Input; set => ((IAssetEditor<ImageAsset>)ViewModel).Input = value; }

        public ImageAsset? Output => ((IAssetEditor<ImageAsset>)ViewModel).Output;

        IAsset? IAssetEditor.Input { get => ((IAssetEditor)ViewModel).Input; set => ((IAssetEditor)ViewModel).Input = value; }

        IAsset? IAssetEditor.Output => ((IAssetEditor)ViewModel).Output;

        public void Reset()
        {
            ((IAssetEditor)ViewModel).Reset();
        }
    }

    public class ImageAssetEditorPageViewModel : AssetEditorEditDone<ImageAsset>, INotifyPropertyChanged, ICloseSignal<bool?>
    {

        public ILevelLogger? OperationLogger { get; set; }
        public ILevelLogger? TrackLogger { get; set; }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion

        #region 关闭信号
        public event EventHandler<bool?>? OnCloseSignal;

        private EventHandler<object?>? baseInterfaceOnCloseSignal;
        event EventHandler<object?>? ICloseSignal.OnCloseSignal
        {
            add
            {
                baseInterfaceOnCloseSignal += value;
            }

            remove
            {
                baseInterfaceOnCloseSignal -= value;
            }
        }

        protected virtual void TriggerOnCloseSignal(bool? b)
        {
            OnCloseSignal?.Invoke(this, b);
            baseInterfaceOnCloseSignal?.Invoke(this, b);
        }

        public ICommand DoneCloseCommand => new SampleCommand(
            _ =>
            {
                EditDone();
                if (CreateAssetResult.IsSuccess)
                {
                    TriggerOnCloseSignal(true);
                }
            },
            _ => true);
        public ICommand CancelCloseCommand => new SampleCommand(_ => TriggerOnCloseSignal(null), _ => true);

        #endregion


        #region 操作
        public ICommand ResetCommand => new SampleCommand(Reset);

        [MemberNotNull(nameof(CreateAssetResult))]
        public override void EditDone()
        {
            createAsset();
            if (CreateAssetResult.IsSuccess)
            {
                var output = CreateAssetResult.Data!;
                Output = output;
                TriggerOnEditDone(output);
            }
            else
            {
                Output = null;
            }
        }

        public override void Reset()
        {
            base.Reset();

            ImageFileName = null;
            previewFileStream?.Dispose();
            previewFileStream = null;
            Preview = null;
            CreateAssetResult = null;
        }

        #endregion


        #region 图片输入

        private string? imageFileName;
        public string? ImageFileName
        {
            get => imageFileName;
            set
            {
                imageFileName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasImageFileName));
            }
        }

        public bool HasImageFileName { get => imageFileName != null; }

        public ICommand SelectImageFileCommand => new SampleCommand(selectImageFile);

        public void selectImageFile()
        {
            OperationLogger?.Info("选择图片文件作为图片资源");
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Multiselect = false,
            };
            if (dialog.ShowDialog() != true)
            {
                TrackLogger?.Info("未选择图片文件");
                ImageFileName = null;
                return;
            }
            else
            {
                OperationLogger?.Info($"选择图片文件: {dialog.FileName}");
                ImageFileName = dialog.FileName;
                CreateAssetResult = null;

                try
                {
                    if (previewFileStream != null)
                    {
                        previewFileStream.Dispose();
                    }
                    previewFileStream = File.OpenRead(ImageFileName);
                    Preview = BitmapFrame.Create(previewFileStream, BitmapCreateOptions.None, BitmapCacheOption.None);

                }
                catch (Exception ex)
                {
                    TrackLogger?.Error("加载文件为预览图的过程发生异常", ex);

                    previewFileStream?.Dispose();
                }
            }

        }
        private OperationResultEx<ImageAsset> openFileAsImageAsset(string fileName)
        {
            try
            {
                using (FileStream fs = File.OpenRead(fileName))
                {
                    ImageAssetWriteReadImpl wrImpl = new();
                    var loadResult = wrImpl.LoadFrom(fs);
                    if (loadResult.IsFailure)
                    {
                        return (loadResult, null);
                    }
                    if (loadResult.Data == null)
                    {
                        return "加载成功, 但仍取得 null 值! ";
                    }
                    return loadResult.Data;

                }
            }
            catch (Exception ex)
            {
                return ex;
            }
        }


        private FileStream? previewFileStream;

        private BitmapSource? preview;
        public BitmapSource? Preview 
        {
            get => preview;
            set
            {
                preview = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region 创建为资源


        public ICommand CreateAssetCommand => new SampleCommand(createAsset);

        [MemberNotNull(nameof(CreateAssetResult))]
        private void createAsset()
        {
            if (CreateAssetResult != null) return;
            if (ImageFileName.IsEmpty())
            {
                CreateAssetResult = (OperationResultEx<ImageAsset>)"当前图片文件名输入为空! ";
            }
            else
            {
                CreateAssetResult = openFileAsImageAsset(ImageFileName);
            }

        }

        private IOperationResultEx<ImageAsset>? createAssetResult;
        public IOperationResultEx<ImageAsset>? CreateAssetResult
        {
            get => createAssetResult;
            set
            {
                createAssetResult = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasCreateAssetResult));
                OnPropertyChanged(nameof(ResultIsSuccessStr));
                OnPropertyChanged(nameof(ResultIsFailure));
                OnPropertyChanged(nameof(ResultFailureStr));
                OnPropertyChanged(nameof(ResultHasException));
                OnPropertyChanged(nameof(ResultExceptionStr));
            }
        }

        public bool HasCreateAssetResult { get => CreateAssetResult != null; }

        public string ResultIsSuccessStr { get => CreateAssetResult == null ? "无结果" : (CreateAssetResult.IsSuccess ? "成功" : "失败"); }
        public bool ResultIsFailure { get => CreateAssetResult?.IsFailure ?? false; }
        public string ResultFailureStr { get => CreateAssetResult?.FailureReason ?? string.Empty; }
        public bool ResultHasException { get => CreateAssetResult?.Exception is not null; }
        public string ResultExceptionStr { get => CreateAssetResult?.Exception?.ToString() ?? string.Empty; }
        #endregion


        protected override ImageAsset ConvertToOutput()
        {
            createAsset();
            if (CreateAssetResult.IsSuccess)
            {
                return CreateAssetResult.Data!;
            }
            else
            {
                throw new OperationFailureException(CreateAssetResult);
            }
        }
    }


}
