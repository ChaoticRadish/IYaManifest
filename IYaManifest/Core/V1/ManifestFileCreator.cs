using Common_Util.Data.Exceptions;
using Common_Util.Data.Struct;
using Common_Util.IO;
using Common_Util.Streams;
using Common_Util.String;
using Common_Util.Xml;
using IYaManifest.Core.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace IYaManifest.Core.V1
{
    public class ManifestFileCreator : ManifestFileCreatorBase
    {
        public override byte Version => 1;

        public override uint AppMark { get; set; }

        public required Manifest Manifest { get; set; }


        public required AssetWriteReader AssetWriteReader { get; set; }

        /// <summary>
        /// 清单头的字符集, 默认使用 <see cref="Encoding.UTF8"/>
        /// </summary>
        public Encoding HeadEncoding { get; set; } = Encoding.UTF8;


        protected override async ValueTask CreateImplAsync(Stream dest)
        {
            /* 简写注释: TF => TempFile
             * 
             */

            using TempFileHelper.TempFile manifestAreaTF = await TempFileHelper.NewTempFileAsync();
            using TempFileHelper.TempFile innerDataTF = await TempFileHelper.NewTempFileAsync();    // 最终会被写入清单文件内, 清单区域内清单项中的数据
            using TempFileHelper.TempFile dataAreaTF = await TempFileHelper.NewTempFileAsync();     // 最终会被写入清单文件内, 数据区域的数据

            using (Stream dataAreaWriteTFStream = dataAreaTF.OpenStream())
            {
                using (Stream innerDataWriteTFStream = innerDataTF.OpenStream())
                {
                    WriteDataToTempFile(dataAreaWriteTFStream, innerDataWriteTFStream);
                }
            }

            using (Stream manifestAreaWriteTFStream = manifestAreaTF.OpenStream())
            {
                using (Stream dataAreaTFStream = dataAreaTF.OpenStream())
                {
                    using (Stream innerDataTFStream = innerDataTF.OpenStream())
                    {
                        WriteHeadToTempFileAsXml(manifestAreaWriteTFStream, dataAreaTFStream, innerDataTFStream);
                    }
                }
            }

            using Stream dataAreaReadTFStream = dataAreaTF.OpenStream();
            using Stream manifestAreaReadTFStream = manifestAreaTF.OpenStream();
            await ManifestFileCreatorDefaultImpls.FromStreamAsync(manifestAreaReadTFStream, dataAreaReadTFStream, Version, AppMark, dest);

        }

        /// <summary>
        /// 将数据暂存到临时文件. 同时各项对应的起点, 长度, 将被更新到清单项中.
        /// <para>会被暂存的类型: </para>
        /// <para><see cref="Enums.AssetDataStorageModeEnum.ManifestData"/>: 暂存到传入的第一个流, 即参数 dataAreaCacheStream</para>
        /// <para><see cref="Enums.AssetDataStorageModeEnum.InManifest"/>: 暂存到传入的第二个流, 即参数 innerDataCacheStream, 对应起点, 长度同样会被更新到清单项中, 后续需要将清单项中的数值重置为 -1, 因为这只是临时值! </para>
        /// </summary>
        /// <param name="dataAreaCacheStream"></param>
        /// <param name="innerDataCacheStream"></param>
        /// <exception cref="OperationFailureException">资源写入流失败时, 将会弹出这类型的异常</exception>
        private void WriteDataToTempFile(Stream dataAreaCacheStream, Stream innerDataCacheStream)
        {
            foreach (var item in Manifest.Items)
            {
                long start;
                Stream stream;

                switch (item.StorageMode)
                {
                    case Enums.AssetDataStorageModeEnum.ManifestData:
                        start = dataAreaCacheStream.Position;
                        stream = dataAreaCacheStream;
                        break;
                    case Enums.AssetDataStorageModeEnum.InManifest:
                        start = innerDataCacheStream.Position;
                        stream = innerDataCacheStream;
                        break;

                    default:
                    case Enums.AssetDataStorageModeEnum.Outside:
                        item.LocationStart = -1;
                        item.LocationLength = -1;
                        continue;
                }

                using (OffsetWrapperStream offsetWrapper = new(stream, start, null))
                {
                    if (item.AssetReference == null)
                    {
                        throw new OperationFailureException((OperationResult)"存在资源引用为 null 的清单项! ");
                    }
                    var writeResult = AssetWriteReader.WriteTo(item.AssetType, item.AssetReference, offsetWrapper);
                    if (writeResult.IsFailure)
                    {
                        throw new OperationFailureException(writeResult);
                    }

                    offsetWrapper.Flush();

                    long length = offsetWrapper.Length;

                    item.LocationStart = start;
                    item.LocationLength = length;
                }

            }
        }

        /// <summary>
        /// 将头以XML形式写入临时文件
        /// </summary>
        /// <param name="tempFileStream">待写入清单 XML 文件数据的流</param>
        /// <param name="dataAreaCacheStream">清单文件内数据区域的数据暂存流</param>
        /// <param name="innerDataCacheStream">嵌入 XML 内的数据暂存流</param>
        private void WriteHeadToTempFileAsXml(Stream tempFileStream, Stream dataAreaCacheStream, Stream innerDataCacheStream)
        {
            System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create();

            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true,
                Indent = true,
            };

            using XmlWriter writer = XmlWriter.Create(tempFileStream, settings);

            writer.WriteStartElement(nameof(Manifest));

            // 头信息写入
            XmlStreamHelper.Write(writer, typeof(ManifestHead), Manifest.Head, nameof(Manifest.Head));

            // 清单项写入
            writer.WriteStartElement(nameof(Manifest.Items));
            foreach (var item in Manifest.Items)
            {
                XmlStreamHelper.ExtraPropertyElementCollection? extraProperty = null;

                Stream? stream = null;

                item.MD5 = [];
                switch (item.StorageMode)
                {
                    case Enums.AssetDataStorageModeEnum.InManifest:
                        if (item.LocationStart >= 0 && item.LocationLength > 0)
                        {
                            stream = new OffsetWrapperStream(innerDataCacheStream, item.LocationStart, item.LocationLength);
                            // 计算MD5值
                            item.MD5 = md5.ComputeHash(stream);
                            stream.Seek(0, SeekOrigin.Begin);

                            // 重置清单项值, 实际不需要使用这两个值, 重置为 -1 避免误导. 
                            item.LocationStart = -1;
                            item.LocationLength = -1;
                        }
                        extraProperty = new()
                        {
                            { ManifestItem.INNER_DATA_TAG_NAME, stream ?? Stream.Null }
                        };
                        break;
                    case Enums.AssetDataStorageModeEnum.ManifestData:
                        if (item.LocationStart >= 0 && item.LocationLength > 0)
                        {
                            stream = new OffsetWrapperStream(dataAreaCacheStream, item.LocationStart, item.LocationLength);
                            // 计算MD5值
                            item.MD5 = md5.ComputeHash(stream);
                            stream.Seek(0, SeekOrigin.Begin);
                        }
                        break;
                }

                XmlStreamHelper.Write(writer, typeof(ManifestItem), item, extraProperty: extraProperty);

                stream?.Dispose();
            }
            writer.WriteEndElement();

            writer.WriteEndElement();


        }

    }
}
