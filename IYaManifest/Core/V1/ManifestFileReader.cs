using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Common_Util.Data.Exceptions;
using Common_Util.Data.Struct;
using Common_Util.IO;
using Common_Util.Streams;
using Common_Util.Xml;
using IYaManifest.Core.Base;
using IYaManifest.Defines;
using IYaManifest.Enums;
using IYaManifest.Interfaces;
using static Common_Util.Module.LayerComponentBaseLong;

namespace IYaManifest.Core.V1
{
    /// <summary>
    /// V1 版本的清单文件读写器, 会尝试将数据流读取为 <see cref="Manifest"/>
    /// </summary>
    public class ManifestFileReader : ManifestFileReaderBase, IManifestFileReader<ManifestHead, ManifestItem>
    {
        private readonly ManifestFileReaderContext context;
        private string 所在文件目录名;
        private string 所在文件目录路径;
        private DirectoryInfo 所在文件目录;



        public ManifestFileReader(ManifestFileReaderContext context)
        {
            this.context = context;
            所在文件目录名 = Path.GetDirectoryName(context.FilePath) ?? throw new ArgumentException("未能取得清单文件对应文件目录");
            所在文件目录 = new DirectoryInfo(所在文件目录名);
            所在文件目录路径 = 所在文件目录名 + "\\";
        }
        #region 初始化参数

        /// <summary>
        /// 资源加载模式的选择器, 其中的选择方法将被单线程调用. 
        /// </summary>
        public required AssetLoadModeSelector AssetLoadModeSelector { get; set; }

        #endregion

        public override async Task<IOperationResult<IManifest<THead, TItem>>> ReadAsync<THead, TItem>()
        {
            this.CheckTypeThrowException(typeof(THead), typeof(TItem));

            var readResult = await readAsyncImpl();
            if (readResult.IsFailure)
            {
                return OperationResultHelper.Failure<OperationResult<IManifest<THead, TItem>>>(readResult);
            }
            else
            {
                IManifest<ManifestHead, ManifestItem>? data = readResult.Data;
                OperationResult<IManifest<THead, TItem>> output = new()
                {
                    IsSuccess = true,
                    Data = (IManifest<THead, TItem>?)data,
                    SuccessInfo = readResult.SuccessInfo,
                };
                return output;
            }
        }
        protected virtual Task<IOperationResult<Manifest>> readAsyncImpl()
        {
            OperationResult<Manifest> result;
            try
            {
                result = _readImpl1();
                if (result.IsFailure)
                {
                    AssetLoadModeSelector.ManifestReadFailure(result);
                }
            }
            catch (Exception ex)
            {
                result = $"发生未处理异常! {ex.Message}";
                AssetLoadModeSelector.ManifestReadFailure(result);
                throw;
            }
            return Task.FromResult<IOperationResult<Manifest>>(result);
        }

        private OperationResult<Manifest> _readImpl1()
        {
            using var fs1 = File.OpenRead(context.FilePath);
            using var manifestStream = new OffsetWrapperStream(fs1, context.Head.ManifestStart, context.Head.ManifestLength);

            using var fs2 = File.OpenRead(context.FilePath);
            using var dataStream = new OffsetWrapperStream(fs2, (long)context.Head.DataStart, (long)context.Head.DataLength);

            using var xmlReader = XmlReader.Create(manifestStream);

            bool findFlag;
            findFlag = XmlStreamHelper.ReadUntilFindElementNode(xmlReader, nameof(Manifest), 1);
            if (!findFlag) return $"XML读取清单区域时, 未找到 {nameof(Manifest)} 节点";
            findFlag = XmlStreamHelper.ReadUntilFindElementNode(xmlReader, nameof(Manifest.Head), 1);
            if (!findFlag) return $"XML读取清单区域时, 未找到 {nameof(Manifest.Head)} 节点";

            if (XmlStreamHelper.ReadAs(
                xmlReader, typeof(ManifestHead),
                existElementTag: false,
                needReadToElementEnd: true) is not ManifestHead manifestHead)
                return $"未能将 {nameof(Manifest.Head)} 节点转化为 {typeof(ManifestHead)}";

            manifestHead.AbsolutePath = Path.GetFullPath(context.FilePath);

            findFlag = XmlStreamHelper.ReadUntilFindElementNode(xmlReader, nameof(Manifest.Items), 1);
            if (!findFlag) return $"XML读取清单区域时, 未找到 {nameof(Manifest.Items)} 节点";

            AssetLoadModeSelector.BeforeReadItems(manifestHead);
            List<ManifestItem> items = new List<ManifestItem>();

            try
            {
                while (XmlStreamHelper.ReadUntilFindElementNode(xmlReader, nameof(ManifestItem), 1))
                {
                    TempFileHelper.TempFile? itemDataTempFile = null;
                    if (XmlStreamHelper.ReadAs(
                        xmlReader, typeof(ManifestItem),
                        existElementTag: false,
                        needReadToElementEnd: false,
                        extraPropertyArgs: new()
                        {
                            AppendAfterReadAttributes = (dic) =>
                            {
                                IEnumerable<string>? append = null;
                                if (dic.TryGetValue(nameof(ManifestItem.StorageMode), out string? value))
                                {
                                    if (value != null && Common_Util.EnumHelper.TryConvert<Enums.AssetDataStorageModeEnum>(value, out var mode))
                                    {
                                        if (mode == Enums.AssetDataStorageModeEnum.InManifest)
                                        {
                                            append = [ManifestItem.INNER_DATA_TAG_NAME];
                                        }
                                    }
                                }
                                return append ?? Array.Empty<string>();
                            },
                            ReadStream = (key, tempFile) =>
                            {
                                switch (key)
                                {
                                    case ManifestItem.INNER_DATA_TAG_NAME:
                                        {
                                            itemDataTempFile = tempFile;
                                            return false;
                                        }
                                }
                                return true;
                            }
                        }) is not ManifestItem item)
                        continue;

                    Stream assetDataStream;
                    long dataLength;

                    switch (item.StorageMode)
                    {
                        case Enums.AssetDataStorageModeEnum.InManifest:
                            {
                                if (itemDataTempFile == null)
                                {
                                    assetDataStream = new MemoryStream();
                                    dataLength = 0;
                                }
                                else
                                {
                                    assetDataStream = itemDataTempFile.Value.OpenStream();
                                    dataLength = new FileInfo(itemDataTempFile.Value.Path).Length;
                                }
                            }
                            break;
                        case Enums.AssetDataStorageModeEnum.ManifestData:
                            {
                                assetDataStream = new OffsetWrapperStream(dataStream, item.LocationStart, item.LocationLength);
                                dataLength = item.LocationLength;
                            }
                            break;
                        case Enums.AssetDataStorageModeEnum.Outside:
                            {
                                if (item.OutsidePath != null)
                                {
                                    string path = PathHelper.GetAbsolutePath(所在文件目录路径, item.OutsidePath);
                                    if (File.Exists(path))
                                    {
                                        dataLength = new FileInfo(path).Length;
                                        assetDataStream = File.OpenRead(path);
                                        break;
                                    }
                                }
                                assetDataStream = new MemoryStream();
                                dataLength = 0;
                            }
                            break;
                        default:
                            return $"未受支持的数据存储类型: {item.StorageMode}";
                    }

                    if (item.MD5 != null && item.MD5.Length > 0)
                    {
                        MD5 md5 = MD5.Create();
                        var computeValue = md5.ComputeHash(assetDataStream);
                        if (!item.MD5.SequenceEqual(computeValue)) 
                        {
                            return $"存在资源 MD5 值与计算值不匹配的情况: {item.AssetId}";
                        }
                    }

                    try
                    {
                        AssetLoadModeSelector.Handle(item, assetDataStream, dataLength);
                    }
                    finally
                    {
                        assetDataStream.Dispose();
                        if (itemDataTempFile != null)
                        {
                            itemDataTempFile.Value.Dispose();
                        }
                    }



                    items.Add(item);
                }
            }
            catch (OperationFailureException ex)
            {
                return "读取清单项过程中出现失败操作: " + ex.Result.FailureReason;
            }

            var manifestItemArray = items.ToArray();

            AssetLoadModeSelector.AfterReadItems(manifestItemArray);

            Manifest output = new()
            {
                Head = manifestHead,
                Items = manifestItemArray,
            };
            return output;
        }
    }

    /// <summary>
    /// V1 版本的清单文件读写器所需的上下文
    /// </summary>
    public struct ManifestFileReaderContext
    {
        /// <summary>
        /// 清单文件的路径 (文件本身, 而不是所在目录)
        /// </summary>
        public string FilePath;

        /// <summary>
        /// 文件内容的偏移量, 一般情况下是 0, 
        /// <para>如果清单被嵌套在另一个清单的数据段内, 则需要计算这个清单在整个文件内的偏移量</para>
        /// </summary>
        public int ContentOffset;

        /// <summary>
        /// 清单文件头
        /// </summary>
        public ManifestFileHead Head;

        public static ManifestFileReaderContext From(ManifestFileReadHelper.ReadContext context)
        {
            return new()
            {
                FilePath = context.FilePath,
                Head = context.FileHead,
                ContentOffset = context.ContentOffset,
            };
        }

    }
}
