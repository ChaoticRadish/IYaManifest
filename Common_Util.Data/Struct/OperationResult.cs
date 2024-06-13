using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Common_Util.Data.Struct
{
    /// <summary>
    /// 简单的操作结果接口
    /// </summary>
    public interface IOperationResult
    {
        /// <summary>
        /// 成功了?
        /// </summary>
        bool IsSuccess { get; set; }
        /// <summary>
        /// 失败了?
        /// </summary>
        bool IsFailure { get; set; }

        /// <summary>
        /// 失败原因
        /// </summary>
        string? FailureReason { get; set; }

        /// <summary>
        /// 成功信息
        /// </summary>
        string? SuccessInfo { get; set; }

    }
    /// <summary>
    /// 附带数据的操作结果接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOperationResult<T> : IOperationResult
    {
        /// <summary>
        /// 操作结果附带的数据
        /// </summary>
        T? Data { get; set; }
    }

    public static class OperationResultHelper
    {
        /// <summary>
        /// 尝试执行指定的可以返回操作结果的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="catchFunc"></param>
        /// <param name="finallyAction"></param>
        /// <returns></returns>
        public static T TryDo<T>(Func<T> body, Func<Exception, T>? catchFunc = null, Action? finallyAction = null)
            where T : IOperationResult, new()
        {
            try
            {
                return body();
            }
            catch (Exception ex)
            {
                if (catchFunc != null)
                {
                    return catchFunc(ex);
                }
                else
                {
                    return Failure<T>("发生异常: " + ex.Message);
                }
            }
            finally
            {
                finallyAction?.Invoke();
            }
        }

        /// <summary>
        /// 尝试执行数次指定的方法, 直到成功或者达到最大尝试次数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="maxCount"></param>
        /// <param name="afterDo">输入参数0: 对应执行轮次的执行结果, 参数1: 当前执行轮次序号; 返回结果: 是否提前结束</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DoWhile<T>(Func<T> body, int maxCount, Func<T, int, bool>? afterDo = null)
            where T : IOperationResult
        {
            T output;
            int index = 0;
            do
            {
                output = body();
                if (afterDo != null)
                {
                    bool advanceEnd = afterDo(output, index);
                    if (advanceEnd)
                    {
                        return output;
                    }
                }
                index++;
            } while (output.IsFailure && index < maxCount);
            return output;
        }

        /// <summary>
        /// 尝试执行数次指定的方法, 直到成功或者达到最大尝试次数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="maxCount"></param>
        /// <param name="afterDo">输入参数0: 对应执行轮次的执行结果, 参数1: 当前执行轮次序号; 返回结果: 是否提前结束</param>
        /// <returns></returns>
        public static async Task<T> DoWhileAsync<T>(Func<Task<T>> body, int maxCount, Func<T, int, Task<bool>>? afterDo = null)
            where T : IOperationResult
        {
            T output;
            int index = 0;
            do
            {
                output = await body();
                if (afterDo != null)
                {
                    bool advanceEnd = await afterDo(output, index);
                    if (advanceEnd)
                    {
                        return output;
                    }
                }
                index++;
            } while (output.IsFailure && index < maxCount);
            return output;
        }


        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时将失败结果返回, 否则返回成功 (<see cref="OperationResult"/> 但是不带成功信息)
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IOperationResult FirstFailure(params Func<IOperationResult>[] funcs)
        {
            return funcs.FirstFailure();
        }

        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时将失败结果返回, 否则返回成功 (<see cref="OperationResult"/> 但是不带成功信息)
        /// </summary>
        /// <param name="funcs"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IOperationResult FirstFailure<TResult>(params Func<TResult>[] funcs)
            where TResult : IOperationResult
        {
            return funcs.FirstFailure();
        }

        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时, 将失败结果及对应的标记组成元组返回, 否则返回成功 ( (default(TFlag), <see cref="OperationResult"/> ) 但是不带成功信息 )
        /// </summary>
        /// <typeparam name="TFlag">方法标记类型</typeparam>
        /// <param name="funcArr"></param>
        /// <returns>失败时: flag: 导致失败的方法对应的标记. operationResult: 导致失败的方法的返回数据</returns>
        public static (TFlag? flag, IOperationResult operationResult) FirstFailure<TFlag>(params (TFlag flag, Func<IOperationResult> func)[] funcArr)
        {
            IOperationResult? result = null;
            foreach (var (flag, func) in funcArr)
            {
                result = func.Invoke();
                if (!result.IsSuccess)
                {
                    return (flag, result);
                }
            }
            return (default(TFlag?), OperationResult.Success);
        }
        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时, 将失败结果及对应的标记组成元组返回, 否则返回成功 ( (default(TFlag), <see cref="OperationResult"/> ) 但是不带成功信息 )
        /// </summary>
        /// <typeparam name="TFlag">方法标记类型</typeparam>
        /// <param name="funcArr"></param>
        /// <returns>失败时: flag: 导致失败的方法对应的标记. operationResult: 导致失败的方法的返回数据</returns>
        public static (TFlag? flag, IOperationResult operationResult) FirstFailure<TFlag, TResult>(params (TFlag flag, Func<TResult> func)[] funcArr)
            where TResult : IOperationResult
        {
            IOperationResult? result = null;
            foreach (var (flag, func) in funcArr)
            {
                result = func.Invoke();
                if (!result.IsSuccess)
                {
                    return (flag, result);
                }
            }
            return (default(TFlag?), OperationResult.Success);
        }
        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时, 将失败结果及对应的标记组成元组返回, 否则返回成功 ( (default(TFlag), <see cref="OperationResult"/> ) 但是不带成功信息 ), 异步版本
        /// </summary>
        /// <typeparam name="TFlag">方法标记类型</typeparam>
        /// <param name="funcArr"></param>
        /// <returns>失败时: flag: 导致失败的方法对应的标记. operationResult: 导致失败的方法的返回数据</returns>
        public static async Task<(TFlag? flag, IOperationResult operationResult)> FirstFailureAsync<TFlag>(params (TFlag flag, Func<Task<IOperationResult>> func)[] funcArr)
        {
            IOperationResult? result = null;
            foreach (var (flag, func) in funcArr)
            {
                result = await func.Invoke();
                if (!result.IsSuccess)
                {
                    return (flag, result);
                }
            }
            return (default(TFlag?), OperationResult.Success);
        }
        /// <summary>
        /// 按枚举顺序执行传入方法集合, 直到遇到出现任意失败项时, 将失败结果及对应的标记组成元组返回, 否则返回成功 ( (default(TFlag), <see cref="OperationResult"/> ) 但是不带成功信息 ), 异步版本
        /// </summary>
        /// <typeparam name="TFlag">方法标记类型</typeparam>
        /// <typeparam name="TResult">统一的结果类型</typeparam>
        /// <param name="funcArr"></param>
        /// <returns>失败时: flag: 导致失败的方法对应的标记. operationResult: 导致失败的方法的返回数据</returns>
        public static async Task<(TFlag? flag, IOperationResult operationResult)> FirstFailureAsync<TFlag, TResult>(params (TFlag flag, Func<Task<TResult>> func)[] funcArr)
            where TResult : IOperationResult
        {
            IOperationResult? result = null;
            foreach (var (flag, func) in funcArr)
            {
                result = await func.Invoke();
                if (!result.IsSuccess)
                {
                    return (flag, result);
                }
            }
            return (default(TFlag?), OperationResult.Success);
        }

        /// <summary>
        /// 根据一个操作结果, 将一个数据包装为 <see cref="OperationResult{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="data"></param>
        /// <param name="defaultFailureInfo">如果操作结果没有失败信息, 则填入此默认值</param>
        /// <returns></returns>
        public static OperationResult<T> Wrapper<T>(this IOperationResult result, T data, string defaultFailureInfo = "")
        {
            if (result.IsSuccess)
            {
                return data;
            }
            else
            {
                return result.FailureReason ?? defaultFailureInfo;
            }
        }

        /// <summary>
        /// 成功的
        /// </summary>
        public static T Success<T>() where T : IOperationResult, new()
        {
            return new T()
            {
                IsSuccess = true,
                FailureReason = null,
            };
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static T Failure<T>(string? reason) where T : IOperationResult, new()
        {
            return new T()
            {
                IsSuccess = false,
                FailureReason = reason
            };
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static T Failure<T>(object? reason) where T : IOperationResult, new()
        {
            return Failure<T>(reason?.ToString());
        }
        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static T Failure<T>(OperationResult child, string? childDesc = null) where T : IOperationResult, new()
        {
            string? failedStr = child.FailureReason;
            childDesc = childDesc?.Trim();
            if (!string.IsNullOrEmpty(childDesc))
            {
                failedStr = childDesc + ": " + failedStr;
            }
            return Failure<T>(failedStr);
        }
        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static T Failure<T, TData>(OperationResult<TData> child, string? childDesc = null) where T : IOperationResult, new()
        {
            string? failedStr = child.FailureReason;
            childDesc = childDesc?.Trim();
            if (!string.IsNullOrEmpty(childDesc))
            {
                failedStr = childDesc + ": " + failedStr;
            }
            return Failure<T>(failedStr);
        }


        /// <summary>
        /// 成功的
        /// </summary>
        public static T Success<T, TData>(TData? obj) where T : IOperationResult<TData>, new()
        {
            return new T()
            {
                Data = obj,
                IsSuccess = true,
                FailureReason = null,
            };
        }
        /// <summary>
        /// 成功的
        /// </summary>
        public static T Success<T, TData>(TData? obj, string? successInfo) where T : IOperationResult<TData>, new()
        {
            return new T()
            {
                Data = obj,
                IsSuccess = true,
                SuccessInfo = successInfo,
                FailureReason = null,
            };
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static T Failure<T, TData>(string? reason) where T : IOperationResult<TData>, new()
        {
            return new T()
            {
                IsSuccess = false,
                FailureReason = reason
            };
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static T Failure<T, TData>(object? reason) where T : IOperationResult<TData>, new()
        {
            return Failure<T, TData>(reason?.ToString());
        }
        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static T Failure<T, TData>(OperationResult<T> child, string? childDesc = null) where T : IOperationResult<TData>, new()
        {
            string? failedStr = child.FailureReason;
            childDesc = childDesc?.Trim();
            if (!string.IsNullOrEmpty(childDesc))
            {
                failedStr = childDesc + ": " + failedStr;
            }
            return Failure<T, TData>(failedStr);
        }


    }


    public struct OperationResult : IOperationResult
    {
        public bool IsSuccess { get; set; }
        public bool IsFailure { get => !IsSuccess; set => IsSuccess = !value; }
        public string? SuccessInfo { get; set; }
        public string? FailureReason { get; set; }

        public override readonly string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('<').Append(IsSuccess ? "成功" : "失败").Append('>');
            if (IsSuccess)
            {
                if (!SuccessInfo.IsEmpty())
                {
                    builder.Append(' ').Append(SuccessInfo);
                }
            }
            else
            {
                if (!FailureReason.IsEmpty())
                {
                    builder.Append(' ').Append(FailureReason);
                }
            }
            return builder.ToString();
        }


        /// <summary>
        /// 成功的
        /// </summary>
        public static OperationResult Success => OperationResultHelper.Success<OperationResult>();
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResult Failure(string? reason)
        {
            return OperationResultHelper.Failure<OperationResult>(reason);
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResult Failure(object reason)
        {
            return OperationResultHelper.Failure<OperationResult>(reason);
        }
        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static OperationResult Failure(OperationResult child, string? childDesc = null)
        {
            return OperationResultHelper.Failure<OperationResult>(child, childDesc);
        }
        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static OperationResult Failure<T>(OperationResult<T> child, string? childDesc = null)
        {
            return OperationResultHelper.Failure<OperationResult, T>(child, childDesc);
        }

        #region 隐式转换
        public static implicit operator bool(OperationResult result)
        {
            return result.IsSuccess;
        }
        public static implicit operator OperationResult(bool b)
        {
            return b ? Success : Failure("失败!");
        }
        public static implicit operator OperationResult((bool isSuccess, string? successInfoOrFailureReason) obj)
        {
            return obj.isSuccess ?
                new OperationResult() { IsSuccess = true, SuccessInfo = obj.successInfoOrFailureReason }
                :
                Failure(obj.successInfoOrFailureReason);
        }
        public static implicit operator OperationResult(string failureReason)
        {
            return Failure(failureReason);
        }
        #endregion

    }
    /// <summary>
    /// 附带数据的操作结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct OperationResult<T> : IOperationResult<T>
    {
        public bool IsSuccess { get; set; }
        public bool IsFailure { get => !IsSuccess; set => IsSuccess = !value; }
        public string? SuccessInfo { get; set; }
        public string? FailureReason { get; set; }
        public T? Data { get; set; }

        public override readonly string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('<').Append(IsSuccess ? "成功" : "失败").Append('>');
            if (IsSuccess)
            {
                if (!SuccessInfo.IsEmpty())
                {
                    builder.Append(' ').Append(SuccessInfo);
                }
            }
            else
            {
                if (!FailureReason.IsEmpty())
                {
                    builder.Append(' ').Append(FailureReason);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// 成功的
        /// </summary>
        public static OperationResult<T> Success(T? obj)
            => OperationResultHelper.Success<OperationResult<T>, T>(obj);
        /// <summary>
        /// 成功的
        /// </summary>
        public static OperationResult<T> Success(T? obj, string? info)
            => OperationResultHelper.Success<OperationResult<T>, T>(obj, info);
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResult<T> Failure(string? reason)
            => OperationResultHelper.Failure<OperationResult<T>, T>(reason);
        /// <summary>
        /// 失败的 (附带数据)
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResult<T> Failure((string? reason, T data) args)
        {
            var result = OperationResultHelper.Failure<OperationResult<T>, T>(args.reason);
            result.Data = args.data;
            return result;
        }
        /// <summary>
        /// 失败的
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static OperationResult<T> Failure(object reason)
            => OperationResultHelper.Failure<OperationResult<T>, T>(reason);
        /// <summary>
        /// 由子操作产生的失败
        /// </summary>
        /// <param name="child"></param>
        /// <param name="childDesc">对子操作的描述, 比如大概是做什么的子操作, 这个方法会加上冒号 ": "</param>
        /// <returns></returns>
        public static OperationResult<T> Failure(OperationResult<T> child, string? childDesc = null)
            => OperationResultHelper.Failure<OperationResult<T>, T>(child, childDesc);

        #region 隐式转换
        public static implicit operator OperationResult(OperationResult<T> result)
        {
            return new OperationResult()
            {
                IsSuccess = result.IsSuccess,
                FailureReason = result.FailureReason,
                SuccessInfo = result.SuccessInfo,
            };
        }
        public static implicit operator bool(OperationResult<T> result)
        {
            return result.IsSuccess;
        }
        public static implicit operator OperationResult<T>(T obj)
        {
            return Success(obj);
        }
        public static implicit operator OperationResult<T>(bool b)
        {
            return b ? Success(default) : Failure("失败!");
        }
        public static implicit operator OperationResult<T>(string failureReason)
        {
            return Failure(failureReason);
        }
        public static implicit operator OperationResult<T>((string failureReason, T data) args)
        {
            return Failure(args);
        }

        /// <summary>
        /// 隐式转换
        /// </summary>
        /// <param name="args">参数 info: 成功时作为成功信息, 失败时作为失败原因信息</param>
        public static implicit operator OperationResult<T>((bool b, T? data, string? info) args)
        {
            if (args.b)
            {
                return Success(args.data, args.info);
            }
            else
            {
                return Failure(args.info);
            }
        }
        #endregion
    }


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
