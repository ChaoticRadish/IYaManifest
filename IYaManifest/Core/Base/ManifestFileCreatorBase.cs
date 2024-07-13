using Common_Util.Data.Struct;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Core.Base
{
    /// <summary>
    /// 清单文件创建器
    /// </summary>
    public abstract class ManifestFileCreatorBase
    {
        /// <summary>
        /// 文件格式版本号
        /// </summary>
        public abstract byte Version { get; }

        /// <summary>
        /// 关联应用的标记
        /// </summary>
        public abstract uint AppMark { get; set; }



        #region 创建操作
        /// <summary>
        /// 创建清单文件, 并保存到指定的路径, 如果文件已存在, 则覆盖. 
        /// </summary>
        /// <param name="filePath"></param>
        public virtual async ValueTask CreateAsync(string filePath)
        {
            using FileStream stream = File.Create(filePath);
            await CreateImplAsync(stream);
            await stream.FlushAsync();
        }
        /// <summary>
        /// 创建清单文件, 写入传入的流中. 
        /// </summary>
        /// <param name="stream"></param>
        public virtual async ValueTask CreateToAsync(Stream stream)
        {
            await CreateImplAsync(stream);
        }

        #endregion
        /// <summary>
        /// 创建清单文件, 写入目标流
        /// </summary>
        /// <param name="dest"></param>
        protected abstract ValueTask CreateImplAsync(Stream dest);

    }
}
