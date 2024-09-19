using Common_Util.Extensions;
using Common_Util.IO;
using Common_Util.Log;
using IYaManifest;
using IYaManifest.Core;
using IYaManifest.Extensions;
using IYaManifest.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifestDemo.AppManage
{
    /// <summary>
    /// 映射关系的帮助类
    /// </summary>
    internal static class MappingHelper
    {
        /// <summary>
        /// 存放扩展 DLL 的文件夹路径
        /// </summary>
        public static DirectoryInfo ExDllDirectory { get; }
            = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Extensions"));

        /// <summary>
        /// 扫描并加载 <see cref="ExDllDirectory"/> 下所有扩展 DLL, 追加到默认映射关系上
        /// </summary>
        /// <remarks>
        /// 如果文件夹不存在, 将创建空文件夹
        /// </remarks>
        /// <param name="writeReaderMappingConflictDealMode">
        /// 读写实现的映射关系在追加到默认映射配置表中时, 如果出现冲突, 应使用此处理方式, 如果为 <see langword="null"/>, 
        /// 将在 DLL 提供建议处理方式时, 采用建议的处理方式, 未提供时, 采用 <see cref="Common_Util.Enums.AppendConflictDealMode.Ignore"/>
        /// </param>
        /// <param name="logger">执行过程中使用的日志输出器, 如果为 <see langword="null"/>, 将采用 <see cref="Globals.DefaultLogger"/> 的
        /// <see cref="LevelLoggerCreator.Get(string, string)"/> 方法获取一个日志输出器 </param>
        public static void LoadAllExDll(
            Common_Util.Enums.AppendConflictDealMode? writeReaderMappingConflictDealMode, 
            ILevelLogger? logger)
        {
            logger ??= Globals.DefaultLogger.Get("映射关系", "加载资源扩展 DLL 文件");

            if (!ExDllDirectory.Exists)
            {
                ExDllDirectory.Create();
                return;
            }
            
            foreach (var file in ExDllDirectory.EnumerateFiles().MatchSuffix("dll"))
            {
                string filePath = file.FullName;
                logger.Info($"尝试加载 DLL 文件: {filePath}");
                try
                {
                    AssetWriteReader.DefaultMapping.AppendDll(
                        writeReaderMappingConflictDealMode,
                        Common_Util.Enums.AppendConflictDealMode.Ignore,
                        filePath);

                    PageTypeMapManager.Instance.AddFromDll(filePath);

                    logger.Info($"加载完成");
                }
                catch (Exception ex)
                {
                    logger.Error($"加载 DLL 文件异常", ex);
                }
            }


        }

    }
}
