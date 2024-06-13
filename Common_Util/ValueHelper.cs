using System;
using System.Collections.Generic;
using System.Text;
    
namespace Common_Util
{
    /// <summary>
    /// 值帮助类
    /// </summary>
    public static class ValueHelper
    {
        /// <summary>
        /// True值字符串
        /// </summary>
        public readonly static List<string> TrueStrings = new List<string>
        {
            "是", "是的", "正确", "没错", "1", "true", "ture", "y", "yes", "t", "一",
        };
        /// <summary>
        /// False值字符串
        /// </summary>
        public readonly static List<string> FalseStrings = new List<string>
        {
            "不", "不是", "错误", "0", "false", "flase", "fales", "n", "f", "no", "nope", "",
        };
        /// <summary>
        /// 判断输入的值是否为True值的字符串
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsTrueString(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (TrueStrings.Contains(input.Trim().ToLower()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


    }
}
