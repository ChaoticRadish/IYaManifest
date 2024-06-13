using Common_Util.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Module
{
    /// <summary>
    /// 默认的对象完整信息获取实现
    /// <para>本实现将读取对象的公共实例属性, 将其按以下情况区分属性类型的种类, 生成属性值对应的字符串: </para>
    /// <para>· 非结构体的值类型或枚举: 将使用<see cref="object.ToString"/>转换为字符串</para>
    /// <para>· 字符串: 输出使用双引号包裹的字符串</para>
    /// <para>· 数组/列表: 以集合的形式, 对集合内每个元素使用本实现生成字符串后嵌入输出结果</para>
    /// <para>· 字典: 以键值对的形式, 对字典内的键使用当前的简略信息获取实现(<see cref="ObjectExtensions.BriefInfoString(object)"/>>), 值使用本实现, 各自生成字符串后嵌入输出结果</para>
    /// <para>· 其他类或结构体: 使用本实现生成字符串后嵌入输出结果</para>
    /// </summary>
    internal class FullInfoGetterImplDefault : IFullInfoGetter
    {
        private Dictionary<Type, TypePropertyInfo> _infoDic = new Dictionary<Type, TypePropertyInfo>();
        private const string NULL_STRING = "<null>";
        private const char SPLIT_CHAR = ':';
        private const string TIME_FORMAT = "yyyy-MM-dd hh:mm:ss:fff";

        public string GetFullInfo(object? obj)
        {
            return _getFullInfo(obj, 0, new());
        }

        private readonly object locker_infoType = new object();
        private string _getFullInfo(object? obj, int depth, Stack<object> objStack)
        {
            if (obj == null)
            {
                return NULL_STRING;
            }
            var exist = objStack.FirstOrDefault(i => i == obj);
            if (exist != null)
            {
                int index = objStack.ToList().IndexOf(exist);
                return $"<循环对象 {index} > {exist}";
            }
            objStack.Push(obj);

            try
            {
                Type type = obj.GetType();
                if (!_infoDic.ContainsKey(type))
                {
                    lock (locker_infoType)
                    {
                        if (!_infoDic.ContainsKey(type))
                        {
                            _infoDic.Add(type, new TypePropertyInfo(type));
                        }
                    }
                }

                TypePropertyInfo info = _infoDic[type];

                switch (info.DealType)
                {
                    case DealTypeEnum.ToString:
                        if (obj is DateTime time)
                        {
                            return time.ToString(TIME_FORMAT);
                        }
                        else
                        {
                            return obj.ToString() ?? string.Empty;
                        }
                    case DealTypeEnum.IsString:
                        return $"\"{(string)obj}\"";
                    case DealTypeEnum.Dictionary:
                        {
                            StringBuilder builder = new StringBuilder();
                            IDictionary dic = (IDictionary)obj;
                            if (dic.Count == 0)
                            {
                                return obj.GetType().Name + "[空字典]";
                            }
                            else
                            {
                                var keys = dic.Keys;
                                foreach (var key in keys)
                                {
                                    var value = dic[key];

                                    string keyStr = key?.BriefInfoString() ?? NULL_STRING;
                                    string valueStr = value == null ? NULL_STRING : _getFullInfo(value, depth + 1, objStack);
                                    string tabStr = "\n" + (" ".Repeat(2 + keyStr.Length + 1));
                                    valueStr = valueStr.Replace("\n", tabStr);

                                    builder.Append('[').Append(keyStr).Append(']').Append(SPLIT_CHAR).Append(valueStr).Append('\n');
                                }
                                if (builder.Length > 0)
                                {
                                    builder.Remove(builder.Length - 1, 1);
                                }

                                return builder.ToString();
                            }
                        }
                    case DealTypeEnum.List:
                    case DealTypeEnum.Array:
                        {
                            StringBuilder builder = new StringBuilder();
                            IList list = (IList)obj;
                            if (list.Count == 0)
                            {
                                return obj.GetType().Name + "[空集合]";
                            }
                            else
                            {
                                List<string> values = new List<string>(list.Count);
                                for (int i = 0; i < list.Count; i++)
                                {
                                    object? item = list[i];
                                    values.Add(_getFullInfo(item, depth + 1, objStack));
                                }
                                double averageLength = values.Average(i => i.Length);
                                string split = averageLength <= 10 ? "; " : "\n";
                                for (int i = 0; i < values.Count; i++)
                                {
                                    string str = values[i];
                                    string tabStr = "\n" + (" ".Repeat(2 + i.ToString().Length + 1));
                                    str = str.Replace("\n", tabStr);
                                    builder.Append('[').Append(i).Append(']').Append(SPLIT_CHAR).Append(str).Append(split);
                                }
                                if (builder.Length > 0)
                                {
                                    builder.Remove(builder.Length - 1, 1);
                                }

                                return builder.ToString();
                            }
                        }
                    case DealTypeEnum.Other:
                        if (info.Properties.Length == 0)
                        {
                            return obj.ToString() ?? obj.GetType().Name;
                        }
                        else
                        {
                            StringBuilder builder = new StringBuilder();
                            StringBuilder indexParametersBuilder = new StringBuilder();

                            string str;
                            string tabStr;
                            foreach (PropertyInfo propertyInfo in info.Properties)
                            {
                                var indexParameters = propertyInfo.GetIndexParameters();
                                if (indexParameters.Length > 0)
                                {
                                    indexParametersBuilder.Clear();
                                    for (int i = 0; i < indexParameters.Length; i++)
                                    {
                                        var param = indexParameters[i];
                                        indexParametersBuilder.Append(param.ParameterType.Name).Append(' ').Append(param.Name);
                                        if (i < indexParameters.Length - 1)
                                        {
                                            indexParametersBuilder.Append(", ");
                                        }
                                    }

                                    str = $"索引器[{indexParametersBuilder}]";
                                }
                                else
                                {
                                    var value = propertyInfo.GetValue(obj, null);
                                    str = _getFullInfo(value, depth + 1, objStack);
                                }
                                tabStr = "\n" + (" ".Repeat(propertyInfo.Name.Length + 1));
                                str = str.Replace("\n", tabStr);
                                builder.Append(propertyInfo.Name).Append(SPLIT_CHAR).AppendLine(str);
                            }
                            if (builder.Length > 0)
                            {
                                builder.Remove(builder.Length - 1, 1);
                            }

                            return builder.ToString();
                        }
                    default:
                        throw new NotSupportedException();
                }
            }
            finally
            {
                objStack.Pop();
            }
        }


        class TypePropertyInfo
        {
            public TypePropertyInfo(Type type) 
            {
                Properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                if (type.IsPrimitive || type.IsEnum)
                {
                    DealType = DealTypeEnum.ToString;
                }
                else if (type == typeof(DateTime))
                {
                    DealType = DealTypeEnum.ToString;
                }
                else if (type == typeof(string)) 
                {
                    DealType = DealTypeEnum.IsString;
                }
                else if (type.IsAssignableTo(typeof(IDictionary)))
                {
                    DealType = DealTypeEnum.Dictionary;
                }
                else if (type.IsArray)
                {
                    DealType = DealTypeEnum.Array;
                }
                else if (type.IsAssignableTo(typeof(IList)))
                {
                    DealType = DealTypeEnum.List;
                }
                else if (type.IsAssignableTo(typeof(MemberInfo)) || type.IsAssignableTo(typeof(ParameterInfo)))
                {
                    DealType = DealTypeEnum.ToString;
                }
                else
                {
                    DealType = DealTypeEnum.Other;
                }


            }
            public readonly PropertyInfo[] Properties;


            public readonly DealTypeEnum DealType;
            
        }
        enum DealTypeEnum
        {
            ToString,
            IsString,
            List,
            Array,
            Dictionary,
            Other,
        }
    }
}
