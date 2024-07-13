using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Core
{
    public static class AssetWriteReaderHelper
    {
        /// <summary>
        /// 将流全部读取到一个 byte[]
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ReadAll(Stream stream)
        {
            byte[] output = new byte[stream.Length];
            int temp;
            int index = 0;
            stream.Seek(0, SeekOrigin.Begin);
            while ((temp = stream.ReadByte()) >= 0)
            {
                output[index] = (byte)temp;
                index++;
            }
            return output;
        }

        /// <summary>
        /// 从流当前位置开始读取到结束位置, 存放到一个 byte[]
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ReadToEnd(Stream stream)
        {
            using MemoryStream ms = new();
            byte[] buffer = new byte[1024];
            int temp;
            stream.Seek(0, SeekOrigin.Begin);
            while ((temp = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer.AsSpan(0, temp));
            }
            return ms.ToArray();
        }
    }
}
