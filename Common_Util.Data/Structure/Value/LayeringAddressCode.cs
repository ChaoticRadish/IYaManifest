using Common_Util.Data.Constraint;
using Common_Util.Extensions;
using Common_Util.String;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Data.Structure.Value
{
    /// <summary>
    /// 分层次的类似地址的编码
    /// <para>由一组的 <see cref="TLayer"/> 类型的值组成(至少一个), 当且仅当每一层级的值都相同时, 两个编码才相等 </para>
    /// </summary>
    /// <typeparam name="TLayer">层级标识的类型</typeparam>
    public interface ILayeringAddressCode<TLayer>
    {
        /// <summary>
        /// 每一层级对应的值
        /// </summary>
        TLayer[] LayerValues { get; }

        /// <summary>
        /// 层级数量
        /// </summary>
        int LayerCount { get; }

        /// <summary>
        /// 地址是否表示一个范围
        /// <para>true => 包含此地址之下的所有分支与项的范围</para>
        /// <para>false => 特定某一具体项</para>
        /// </summary>
        bool IsRange { get; }
    }

    /// <summary>
    /// 分层次的类似地址的编码, 其中的层级标识的类型是 <see cref="IStringConveying"/>
    /// </summary>
    /// <typeparam name="TLayer"></typeparam>
    public interface IStringConveyingLayeringAddressCode<TLayer> : ILayeringAddressCode<TLayer>
        where TLayer : IStringConveying
    {

    }



    /// <summary>
    /// 可与字符串互相转换的分层次的类似地址的编码, 层级标识的类型也是可与字符串互相转换的
    /// <para>层级标识转换为字符串时, 需要对部分字符作转义处理</para>
    /// </summary>
    /// <typeparam name="TLayer"></typeparam>
    public struct LayeringAddressCode<TLayer> : IStringConveyingLayeringAddressCode<TLayer>, IStringConveying
        where TLayer : IStringConveying, new()
    {
        #region 转义相关常量
        /// <summary>
        /// 转义字符
        /// </summary>
        public const char EscapeChar = '\\';
        /// <summary>
        /// 分隔不同层级标识的字符
        /// </summary>
        public const char SplitChar = '.';
        /// <summary>
        /// 具体项标识字符, 用于最后一个层级前, 如果最后一个层级前的字符是 '.', 则表示一个范围, 如果是 ':', 则表示具体某个东西
        /// <para>例如: </para>
        /// <para>AAA.BBB.CCC 这一字符串表示地址 AAA.BBB.CCC.* 这一范围</para>
        /// <para>AAA.BBB:CCC 这一字符串表示 AAA.BBB 中的 CCC</para>
        /// </summary>
        public const char ItemMarkChar = ':';

        #endregion

        public TLayer[] LayerValues 
        { 
            get => layerValues ; 
            set 
            { 
                layerValues = value; 
                if (value == null || value.Length == 0)
                {
                    isRange = true;
                }
            } 
        }
        private TLayer[] layerValues;

        public readonly int LayerCount { get => layerValues?.Length ?? 0; }

        public bool IsRange
        {
            get => layerValues == null || layerValues.Length == 0 || isRange;
            set
            {
                if (layerValues == null || layerValues.Length == 0) 
                {
                    isRange = true;
                }
                else
                {
                    isRange = value;
                }
            }
        }
        private bool isRange;

        /// <summary>
        /// 表示一个包含一切的范围
        /// </summary>
        public static LayeringAddressCode<TLayer> All { get; } = new LayeringAddressCode<TLayer>()
        {
            LayerValues = [],
            IsRange = true,
        };

        public void ChangeValue(string value)
        {
            if (value.IsEmpty()) 
            {
                LayerValues = [];
                IsRange = true;
                return;
            }
            var (splitStrs, isRange) = LayeringAddressCodeHelper.AnalysisString(value, EscapeChar, SplitChar, ItemMarkChar);
            LayerValues = splitStrs.Select(StringConveyingHelper.FromString<TLayer>).ToArray();
            IsRange = isRange;

        }

        public string ConvertToString()
        {
            return LayeringAddressCodeHelper.ConvertToString(
                LayerValues, IsRange, 
                i => i.ConvertToString(), 
                EscapeChar, SplitChar, ItemMarkChar);
        }

        #region 隐式转换
        public static implicit operator LayeringAddressCode<TLayer>(string value)
        {
            return StringConveyingHelper.FromString<LayeringAddressCode<TLayer>>(value);
        }
        public static implicit operator LayeringAddressCode<TLayer>(string[] layerAddress)
        {
            return (layerAddress, false);
        }
        public static implicit operator LayeringAddressCode<TLayer>((bool isRange, string[] layerAddress) obj)
        {
            return (obj.layerAddress, obj.isRange);
        }
        public static implicit operator LayeringAddressCode<TLayer>((string[] layerAddress, bool isRange) obj)
        {
            return new LayeringAddressCode<TLayer>()
            {
                LayerValues = obj.layerAddress.Select(StringConveyingHelper.FromString<TLayer>).ToArray(),
                IsRange = obj.isRange,
            };
        }
        public static implicit operator LayeringAddressCode<TLayer>((bool isRange, TLayer[] layerAddress) obj)
        {
            return (obj.layerAddress, obj.isRange);
        }
        public static implicit operator LayeringAddressCode<TLayer>((TLayer[] layerAddress, bool isRange) obj)
        {
            return new LayeringAddressCode<TLayer>()
            {
                LayerValues = obj.layerAddress,
                IsRange = obj.isRange,
            };
        }
        public static implicit operator string(LayeringAddressCode<TLayer> value)
        {
            return value.ConvertToString();
        }
        #endregion
    }

    /// <summary>
    /// 可与字符串互相转换的分层次的类似地址的编码
    /// </summary>
    public struct LayeringAddressCode : ILayeringAddressCode<string>, IStringConveying
    {
        #region 转义相关常量
        /// <summary>
        /// 转义字符
        /// </summary>
        public const char EscapeChar = '\\';
        /// <summary>
        /// 分隔不同层级标识的字符
        /// </summary>
        public const char SplitChar = '.';
        /// <summary>
        /// 具体项标识字符, 用于最后一个层级前, 如果最后一个层级前的字符是 '.', 则表示一个范围, 如果是 ':', 则表示具体某个东西
        /// <para>例如: </para>
        /// <para>AAA.BBB.CCC 这一字符串表示地址 AAA.BBB.CCC.* 这一范围</para>
        /// <para>AAA.BBB:CCC 这一字符串表示 AAA.BBB 中的 CCC</para>
        /// </summary>
        public const char ItemMarkChar = ':';

        #endregion

        public string[] LayerValues
        {
            get => layerValues;
            set
            {
                layerValues = value;
                if (value == null || value.Length == 0)
                {
                    isRange = true;
                }
            }
        }
        private string[] layerValues;

        public readonly int LayerCount { get => layerValues?.Length ?? 0; }

        public bool IsRange
        {
            get => layerValues == null || layerValues.Length == 0 || isRange;
            set
            {
                if (layerValues == null || layerValues.Length == 0)
                {
                    isRange = true;
                }
                else
                {
                    isRange = value;
                }
            }
        }
        private bool isRange;

        /// <summary>
        /// 表示一个包含一切的范围
        /// </summary>
        public static LayeringAddressCode All { get; } = new LayeringAddressCode()
        {
            LayerValues = [],
            IsRange = true,
        };

        public void ChangeValue(string value)
        {
            if (value.IsEmpty())
            {
                LayerValues = [];
                IsRange = true;
                return;
            }

           var (splitStrs, isRange) = LayeringAddressCodeHelper.AnalysisString(value, EscapeChar, SplitChar, ItemMarkChar);
            LayerValues = [.. splitStrs];
            IsRange = isRange;
        }

        public string ConvertToString()
        {
            return LayeringAddressCodeHelper.ConvertToString(
                LayerValues, IsRange,
                i => i,
                EscapeChar, SplitChar, ItemMarkChar);
        }

        #region 隐式转换
        public static implicit operator LayeringAddressCode(string value)
        {
            return StringConveyingHelper.FromString<LayeringAddressCode>(value);
        }
        public static implicit operator LayeringAddressCode(string[] layerAddress)
        {
            return (layerAddress, false);
        }
        public static implicit operator LayeringAddressCode((bool isRange, string[] layerAddress) obj)
        {
            return (obj.layerAddress, obj.isRange);
        }
        public static implicit operator LayeringAddressCode((string[] layerAddress, bool isRange) obj)
        {
            if (obj.layerAddress.Any(i => i.IsEmpty()))
            {
                throw new ArgumentException("地址出现了空值层级! ");
            }
            return new LayeringAddressCode()
            {
                LayerValues = obj.layerAddress,
                IsRange = obj.isRange,
            };
        }
        public static implicit operator string(LayeringAddressCode value)
        {
            return value.ConvertToString();
        }
        #endregion
    }



    internal static class LayeringAddressCodeHelper
    {
        #region 字符串相关
        [MethodImpl(MethodImplOptions.AggressiveInlining)]  
        public static (List<string> splitStrs, bool isRange) AnalysisString(string value, char escapeChar, char splitChar, char itemMarkChar)
        {
            List<string> strs = new();
            StringBuilder sb = new();
            bool hasItemMark = false;
            EscapeHelper.Ergodic(value, escapeChar,
                (c, b) =>
                {
                    if (!b)
                    {
                        if (c == splitChar || c == itemMarkChar)
                        {
                            if (sb.Length == 0)
                            {
                                throw new ArgumentException("出现空的层级标识! ");
                            }
                            else
                            {
                                strs.Add(sb.ToString());
                                sb.Clear();
                            }
                            if (c == itemMarkChar)
                            {
                                if (hasItemMark)
                                {
                                    throw new ArgumentException("出现了两个或以上的类型标识字符! ");
                                }
                                hasItemMark = true;
                            }
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                });
            if (sb.Length == 0)
            {
                throw new ArgumentException("最后一个层级标识是空的! ");
            }
            else
            {
                strs.Add(sb.ToString());
            }
            return (strs, !hasItemMark);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ConvertToString<T>(T[] items, bool isRange, Func<T, string> convertFunc, char escapeChar, char splitChar, char itemMarkChar)
        {
            StringBuilder sb = new();
            if (items == null) return string.Empty;
            for (int i = 0; i < items.Length; i++)
            {
                if (i > 0)
                {
                    if (!isRange && i == items.Length - 1)
                    {
                        sb.Append(itemMarkChar);
                    }
                    else
                    {
                        sb.Append(splitChar);
                    }
                }
                sb.Append(EscapeHelper.AddEscape(convertFunc(items[i]), escapeChar, splitChar, itemMarkChar));
            }
            return sb.ToString();
        }
        #endregion

    }

}
