using Common_Util.Interfaces.Behavior;
using Common_Util.Module.Command;
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
    public abstract class AssetEditorViewModelBase_Clone<TAsset> : AssetEditorCloneToEdit<TAsset>, INotifyPropertyChanged, ICloseSignal<bool?>
        where TAsset : IAsset
    {

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

        public ICommand DoneCloseCommand => new SampleCommand(_ => TriggerOnCloseSignal(true), _ => true);
        public ICommand CancelCloseCommand => new SampleCommand(_ => TriggerOnCloseSignal(null), _ => true);

        #endregion

        #region 操作
        public ICommand ResetCommand => new SampleCommand(Reset);
        #endregion
    }

    public abstract class AssetEditorViewModelBase_EditDone<TAsset> : AssetEditorEditDone<TAsset>, INotifyPropertyChanged, ICloseSignal<bool?>
        where TAsset : IAsset
    {

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
                TriggerOnCloseSignal(true);
            }, 
            _ => true);
        public ICommand CancelCloseCommand => new SampleCommand(_ => TriggerOnCloseSignal(null), _ => true);

        #endregion


        #region 操作
        public ICommand ResetCommand => new SampleCommand(Reset);
        #endregion
    }
}
