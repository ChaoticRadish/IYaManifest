using Common_Util.Data.Struct;
using Common_Util.Interfaces.Behavior;
using Common_Util.Module.Command;
using IYaManifest;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace IYaManifestAssetLibTest.Wpf
{
    public abstract class AssetCreatorViewModelBase<TAsset> : AssetCreatorBase<TAsset>, INotifyPropertyChanged, ICloseSignal<bool?>
        where TAsset : IAsset
    {
        public override TAsset? CreatedAsset { get; protected set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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

        public ICommand DoneCloseCommand => new SampleCommand(_ => TriggerOnCloseSignal(true), _ => true);
        public ICommand CancelCloseCommand => new SampleCommand(_ => TriggerOnCloseSignal(null), _ => true);

        #endregion

        #region 状态

        private bool allowEdit = true;
        /// <summary>
        /// 当前处于允许编辑的状态
        /// </summary>
        public bool AllowEdit { get => allowEdit; protected set { allowEdit = value; OnPropertyChanged(); } }

        private IOperationResultEx? createResult;
        /// <summary>
        /// 创建操作的结果
        /// </summary>
        public IOperationResultEx? CreateResult
        { 
            get => createResult; 
            set 
            {
                createResult = value; 
                OnPropertyChanged(); 
            }
        }
        #endregion

        #region 操作
        public ICommand ResetCommand => new SampleCommand(_ => Reset(), _ => true);
        /// <summary>
        /// 重置当前输入状态
        /// </summary>
        public virtual void Reset()
        {
            AllowEdit = true;
            CreateResult = null;
        }

        public ICommand CreateCommand => new SampleCommand(_ => Create(), _ => true);
        public void Create()
        {
            OperationResultEx<TAsset> result;
            try
            {
                var _r = CreateImpl();
                CreatedAsset = _r.Data;
                {
                    var logger = Globals.DefaultLogger.Get("AssetLibTest", "创建资源");
                    if (CreatedAsset == null)
                    {
                        logger.Info($"资源创建取得 null 值");
                    }
                    else
                    {
                        string messageStr;
                        if (CreatedAsset is IDataStringAsset dsAsset)
                        {
                            messageStr = dsAsset.DataString;
                        }
                        else
                        {
                            messageStr = CreatedAsset?.ToString() ?? "<null>";
                        }
                        logger.Info($"资源创建: ({CreatedAsset?.GetType().Name}) {messageStr}");
                    }
                }
                result = new OperationResultEx<TAsset>()
                {
                    Data = _r.Data,
                    IsSuccess = _r.IsSuccess,
                    FailureReason = _r.FailureReason,
                    SuccessInfo = _r.SuccessInfo,
                };
                if (_r is IOperationResultEx exResult)
                {
                    result.Exception = exResult.Exception;
                }
                TriggerOnCreateDone();
            }
            catch (Exception ex)
            {
                result = ex;
            }
            CreateResult = result;
        }
        protected abstract IOperationResult<TAsset> CreateImpl();

        #endregion

    }
}
