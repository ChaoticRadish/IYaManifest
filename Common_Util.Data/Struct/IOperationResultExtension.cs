using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Struct
{
    public static class IOperationResultExtension
    {
        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时将失败结果返回, 否则返回成功 (<see cref="OperationResult"/> 但是不带成功信息)
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        public static IOperationResult FirstFailure(this IEnumerable<Func<IOperationResult>> funcs)
        {
            IOperationResult? result = null;
            foreach (var func in funcs)
            {
                result = func.Invoke();
                if (!result.IsSuccess)
                {
                    return result;
                }
            }
            return result ?? OperationResult.Success;
        }
        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时将失败结果返回, 否则返回成功 (<see cref="OperationResult"/> 但是不带成功信息)
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        public static IOperationResult FirstFailure<TResult>(this IEnumerable<Func<TResult>> funcs)
            where TResult : IOperationResult
        {
            IOperationResult? result = null;
            foreach (var func in funcs)
            {
                result = func.Invoke();
                if (!result.IsSuccess)
                {
                    return result;
                }
            }
            return result ?? OperationResult.Success;
        }

        /// <summary>
        /// 取得失败信息
        /// </summary>
        /// <param name="result"></param>
        /// <param name="defaultValue">如果失败信息为null, 返回这个值</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string FailureReason(this IOperationResult result, string defaultValue = "")
        {
            return result.FailureReason ?? defaultValue;
        }

        /// <summary>
        /// 尝试执行数次指定的方法, 直到成功或者达到最大尝试次数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="maxCount"></param>
        /// <param name="afterDo">输入参数0: 对应执行轮次的执行结果, 参数1: 当前执行轮次序号; 返回结果: 是否提前结束</param>
        /// <returns></returns>
        public static T DoWhile<T>(this Func<T> func, int maxCount, Func<T, int, bool>? afterDo = null) where T : IOperationResult
        {
            return OperationResultHelper.DoWhile(func, maxCount, afterDo);
        }
        /// <summary>
        /// 尝试执行数次指定的方法, 直到成功或者达到最大尝试次数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="maxCount"></param>
        /// <param name="afterDo">输入参数0: 对应执行轮次的执行结果, 参数1: 当前执行轮次序号; 返回结果: 是否提前结束</param>
        /// <returns></returns>
        public static Task<T> DoWhileAsync<T>(this Func<Task<T>> func, int maxCount, Func<T, int, Task<bool>>? afterDo = null) where T : IOperationResult
        {
            return OperationResultHelper.DoWhileAsync(func, maxCount, afterDo);
        }
    }
}
