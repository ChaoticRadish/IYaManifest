using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// 不为null时调用 <see cref="Enum.ToString()"/>, 为null时返回输入的默认值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="whenNullDefault"></param>
        /// <returns></returns>
        public static string ToString(this Enum? value, string whenNullDefault)
        {
            return value == null ? whenNullDefault : value.ToString();
        }
    }
}
