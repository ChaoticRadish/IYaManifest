using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Defines
{
    public static class DefineHelper
    {
        /// <summary>
        /// 创建一个固定头数组
        /// </summary>
        /// <param name="mark">仅支持 ASCII 码</param>
        /// <returns></returns>
        public static byte[] CreateFixedMarkArray(ReadOnlySpan<char> mark)
        {
            byte[] output = new byte[mark.Length];
            for (int i = 0; i < output.Length; i++)
            {
                if (mark[i] > 255)
                {
                    throw new InvalidOperationException("仅支持 ASCII 码");
                }
                output[i] = (byte) mark[i];
            }
            return output;
        }

    }
}
