using Common_Util.Interfaces.Behavior;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Wpf.ViewModels
{
    /// <summary>
    /// 具有关闭信号的通用 ViewModel
    /// </summary>
    /// <typeparam name="TSignal"></typeparam>
    public class CloseSignalViewModelBase<TSignal> : INotifyPropertyChanged, ICloseSignal<TSignal>
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;


        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion



        #region 页面关闭信号
        public event EventHandler<TSignal?>? OnCloseSignal;

        private event EventHandler<object?>? baseInterfaceOnCloseSignal;

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

        /// <summary>
        /// 触发关闭信号事件
        /// </summary>
        /// <param name="arg"></param>
        protected void TriggerCloseSignal(TSignal arg, object? sender = null)
        {
            OnCloseSignal?.Invoke(sender ?? this, arg);
            baseInterfaceOnCloseSignal?.Invoke(sender ?? this, arg);
        }

        #endregion
    }
}
