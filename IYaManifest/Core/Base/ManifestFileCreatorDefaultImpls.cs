using IYaManifest.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Core.Base
{
    public static class ManifestFileCreatorDefaultImpls
    {
        #region 使用特定的创建方法, 创建一个创建器对象
        public delegate ValueTask CreateImplHandler(byte version, uint appMark, Stream dest);
        /// <summary>
        /// 使用特定的创建方法, 创建一个创建器对象
        /// </summary>
        /// <param name="version">创建器对应的版本号</param>
        /// <param name="impl">创建方法的具体实现</param>
        /// <returns></returns>
        public static ManifestFileCreatorBase GetCreatorFromImpl(byte version, CreateImplHandler impl)
        {
            return new SimpleCreator(version, impl);
        }
        /// <summary>
        /// 简易实现的创建器
        /// </summary>
        /// <param name="version"></param>
        /// <param name="implHandler"></param>
        private class SimpleCreator(byte version, CreateImplHandler implHandler) : ManifestFileCreatorBase
        {
            private readonly CreateImplHandler implHandler = implHandler;

            public override byte Version { get; } = version;

            public override uint AppMark { get; set; }

            protected override ValueTask CreateImplAsync(Stream dest)
            {
                return implHandler(Version, AppMark, dest);
            }
        }

        #endregion

        public static async ValueTask FromStreamAsync(
            Stream ManifestStream, Stream DataStream, 
            byte Version, uint AppMark, 
            Stream dest)
        {
            MD5 md5 = MD5.Create();

            int headSize = Marshal.SizeOf(typeof(ManifestFileHead));

            uint manifestStart = (uint)headSize;
            uint manifestLength = (uint)ManifestStream.Length;
            byte[] manifestMd5 = md5.ComputeHash(ManifestStream);
            ulong dataStart = (ulong)manifestStart + manifestLength;
            ulong dataLength = (ulong)DataStream.Length;
            byte[] dataMd5 = md5.ComputeHash(ManifestStream);

            ManifestFileHead head = new ManifestFileHead()
            {
                Mark = ManifestFileHead.FixedMarkArray,
                ManifestStart = manifestStart,
                ManifestLength = manifestLength,
                ManifestMd5 = manifestMd5,
                DataStart = dataStart,
                DataLength = dataLength,
                DataMd5 = dataMd5,
                Version = Version,
                AppMark = AppMark,
            };
            byte[] headData = new byte[headSize];

            nint ptr = Marshal.AllocHGlobal(headSize);
            Marshal.StructureToPtr(head, ptr, false);
            Marshal.Copy(ptr, headData, 0, headSize);
            Marshal.FreeHGlobal(ptr);

            head.CRC8 = Common_Util.Check.CRCHelper.CRC8(headData.AsSpan(0, headSize - 1));  // 减去一个 byte, 即存放 CRC 校验码的位置
            headData[^1] = head.CRC8;

            dest.Seek(0, SeekOrigin.Begin);
            await dest.WriteAsync(headData);

            byte[] buffer = new byte[1024];
            int readCount;

            ManifestStream.Seek(0, SeekOrigin.Begin);
            while ((readCount = ManifestStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                await dest.WriteAsync(buffer, 0, readCount);
            }

            DataStream.Seek(0, SeekOrigin.Begin);
            while ((readCount = DataStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                await dest.WriteAsync(buffer, 0, readCount);
            }

        }

    }
}
