using Common_Util.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest
{
    public static class Globals
    {
        #region 日志
        /// <summary>
        /// 映射相关的日志
        /// </summary>
        public static ILevelLogger? MappingLogger { get; set; }

        /// <summary>
        /// 测试相关的日志
        /// </summary>
        public static ILevelLogger? TestLogger { get; set; }


        public static ILevelLogger? FileReaderLogger { get; set; }

        public static ILevelLogger? FileCreatorLogger { get; set; }

        #endregion

    }
}
