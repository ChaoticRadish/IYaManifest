using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Interfaces
{
    /// <summary>
    /// 资源编辑器
    /// </summary>
    /// <remarks>
    /// 可以在原有资源的基础上作修改的编辑器, 传入数据与编辑结果在实现上不能是同一个引用
    /// </remarks>
    public interface IAssetEditor
    {
        /// <summary>
        /// 传入的准备编辑的资源
        /// </summary>
        /// <remarks>可能会被多次调用获取, 需要尽量返回一个固定的对象, 而不是每次调用都会取得一个新对象</remarks>
        public IAsset? Input { get; set; }
        /// <summary>
        /// 传出的编辑结果
        /// </summary>
        /// <remarks>
        /// 可能会被多次调用获取, 需要尽量返回一个固定的对象, 而不是每次调用都会取得一个新对象<br/>
        /// </remarks>
        public IAsset? Output { get; }

        /// <summary>
        /// 重置当前编辑状态
        /// </summary>
        public void Reset();

        /// <summary>
        /// 编辑完成事件
        /// </summary>
        public event EventHandler<IAsset> OnEditDone;
    }

    /// <summary>
    /// 专用于特定资源的资源编辑器
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public interface IAssetEditor<TAsset> : IAssetEditor
        where TAsset : IAsset
    {
        /// <summary>
        /// 传入的准备编辑的资源
        /// </summary>
        /// <remarks>可能会被多次调用获取, 需要尽量返回一个固定的对象, 而不是每次调用都会取得一个新对象</remarks>
        public new TAsset? Input { get; set; }
        /// <summary>
        /// 传出的编辑结果
        /// </summary>
        /// <remarks>
        /// 可能会被多次调用获取, 需要尽量返回一个固定的对象, 而不是每次调用都会取得一个新对象<br/>
        /// </remarks>
        public new TAsset? Output { get; }

        public new event EventHandler<TAsset> OnEditDone;
    }

    /// <summary>
    /// 包含接口 <see cref="IAssetEditor{TAsset}"/> 默认实现的抽象基类
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public abstract class AssetEditorBase<TAsset> : IAssetEditor<TAsset>
        where TAsset : IAsset
    {
        public abstract TAsset? Input { get; set; }
        public abstract TAsset? Output { get; protected set; }

        IAsset? IAssetEditor.Input { get => Input; set => Input = value == null ? default : (TAsset)value; }
        IAsset? IAssetEditor.Output { get => Output; }

        public event EventHandler<TAsset>? OnEditDone;

        private event EventHandler<IAsset>? BaseInterfaceOnEditDone;
        event EventHandler<IAsset> IAssetEditor.OnEditDone
        {
            add
            {
                BaseInterfaceOnEditDone += value;
            }

            remove
            {
                BaseInterfaceOnEditDone -= value;
            }
        }

        public abstract void Reset();

        /// <summary>
        /// 使用 <see cref="Output"/> 触发编辑完成事件
        /// </summary>
        protected virtual void TriggerOnEditDone()
        {
            var o = Output;
            ArgumentNullException.ThrowIfNull(o, nameof(Output));
            OnEditDone?.Invoke(this, o);
            BaseInterfaceOnEditDone?.Invoke(this, o);
        }
        /// <summary>
        /// 使用传入的资源, 触发编辑完成事件
        /// </summary>
        /// <param name="asset"></param>
        protected virtual void TriggerOnEditDone(TAsset asset)
        {
            OnEditDone?.Invoke(this, asset);
            BaseInterfaceOnEditDone?.Invoke(this, asset);
        }
    }

    /// <summary>
    /// 克隆对象用以编辑的资源编辑器
    /// </summary>
    /// <remarks>
    /// 在这个实现下, 正常不会有 <see cref="IAssetEditor.OnEditDone"/> 事件发生, <br/>
    /// 因为编辑都应该是实时反应到输出结果的对象上. <br/>
    /// 当然, 如果子类需要触发这个事件, 比如有一个编辑完成的按钮, <br/>
    /// 也可以在按钮点击过程中调用 <see cref="AssetEditorBase{T}.TriggerOnEditDone"/> 以触发事件
    /// </remarks>
    /// <typeparam name="TAsset"></typeparam>
    public abstract class AssetEditorCloneToEdit<TAsset> : AssetEditorBase<TAsset>
        where TAsset : IAsset
    {
        private TAsset? input { get; set; }
        public override TAsset? Input
        {
            get => input; 
            set
            {
                input = value;
                if (input == null)
                {
                    Output = default;
                    HasEditingObject = false;
                }
                else
                {
                    Output = Clone(input);
                    HasEditingObject = true;
                }
                AfterInit();
            }
        }

        /// <summary>
        /// 传出的编辑结果, 同时也是当前编辑对象 <see cref="Editing"/>
        /// </summary>
        public override TAsset? Output { get; protected set; }
        /// <summary>
        /// 当前是否拥有编辑中的对象
        /// </summary>
        /// <remarks>
        /// 当 <see cref="TAsset"/> 是不可空类型时, 直接调用 <see cref="Editing"/> 可能获取到非 <see langword="null"/> 的默认值 <br/>
        /// 所以确定是否拥有值时, 需要使用此字段来判断
        /// </remarks>
        public bool HasEditingObject { get; protected set; }
        /// <summary>
        /// 当前编辑中的资源对象
        /// </summary>
        public TAsset? Editing { get => Output; protected set => Output = value; }

        /// <summary>
        /// 克隆一个新的资源对象用以编辑
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        protected abstract TAsset Clone(TAsset asset);


        #region 初始化与重置
        /// <summary>
        /// 在设置 Input 之后调用
        /// </summary>
        protected virtual void AfterInit()
        {

        }

        /// <summary>
        /// 重置当前编辑状态
        /// </summary>
        /// <remarks>
        /// 如果当前输入对象不为空, 则重新克隆一个新对象用以编辑<br/>
        /// 该方法执行过程中会调用到 <see cref="AfterInit"/>, 因为调用到了 <see cref="Input"/> 的 <see langword="set"/> 
        /// </remarks>
        public override void Reset()
        {
            BeforeResetEditing(Editing);
            Input = input;  // 触发克隆
        }

        /// <summary>
        /// 重置当前编辑对象前调用的方法
        /// </summary>
        /// <param name="asset">在此之前的被编辑对象</param>
        protected virtual void BeforeResetEditing(TAsset? asset) { }

        #endregion


    }

    /// <summary>
    /// 编辑完成后再输出编辑结果的资源编辑器
    /// </summary>
    /// <typeparam name="TAsset"></typeparam>
    public abstract class AssetEditorEditDone<TAsset> : AssetEditorBase<TAsset>
        where TAsset : IAsset
    {
        #region 状态
        /// <summary>
        /// 当前是否处于允许编辑的状态, 在 <see cref="ConvertToOutput"/> 执行过程中为 <see langword="false"/>, 其余时间为 <see langword="true"/>
        /// </summary>
        public virtual bool AllowEdit { get; protected set; } = true;

        private static readonly object ConvertToOutputLocker = new();
        #endregion

        private TAsset? input { get; set; }
        public override TAsset? Input
        {
            get => input;
            set
            {
                input = value;
                Reset();
            }
        }

        /// <summary>
        /// 传出的编辑结果, 同时也是当前编辑对象 <see cref="Editing"/>
        /// </summary>
        public override TAsset? Output { get; protected set; }

        /// <summary>
        /// 重置当前编辑状态, 设置 <see cref="Input"/> 会执行此方法
        /// </summary>
        public override void Reset()
        {
            Output = default;
        }

        /// <summary>
        /// 将当前输入内容 (非 <see cref="Input"/>, 而是具体实现中的输入, 比如属性值等等) 整合为输出结果
        /// </summary>
        /// <remarks>
        /// 此方法将调用 <see cref="AssetEditorBase{TAsset}.TriggerOnEditDone(TAsset)"/>
        /// </remarks>
        public virtual void EditDone()
        {
            TAsset output = InvokeConvertToOutput();
            Output = output;
            TriggerOnEditDone(output);
        }

        /// <summary>
        /// 调用 <see cref="ConvertToOutput"/>, 这个过程中会更新 <see cref="AllowEdit"/>
        /// </summary>
        /// <returns></returns>
        protected TAsset InvokeConvertToOutput()
        {
            lock (ConvertToOutputLocker)
            {
                AllowEdit = false;

                TAsset output;
                try
                {
                    output = ConvertToOutput();
                }
                finally
                {
                    AllowEdit = true;
                }
                return output;
            }
        }
       

        /// <summary>
        /// 将当前输入内容转换为输出结果
        /// </summary>
        /// <returns></returns>
        protected abstract TAsset ConvertToOutput();

    }

    #region 扩展方法
    public static class IAssetEditorExtensions
    {
        /// <summary>
        /// 检查传入的编辑器 <see cref="IAssetEditor.Input"/> 与 <see cref="IAssetEditor.Output"/> 是否同一个对象 (引用相同)
        /// </summary>
        /// <param name="editor"></param>
        /// <returns>
        /// <see langword="true"/> => 检查通过 <br/>
        /// <see langword="true"/> => 检查未通过 <br/>
        /// 其中任意一方为 null 时, 也将通过检查</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CheckSameReferenceError(this IAssetEditor editor)
        {
            var eI = editor.Input;
            var eO = editor.Output;
            if (eI == null || eO == null) return true;
            return !object.ReferenceEquals(eI, eO);
        }
    }
    #endregion
}
