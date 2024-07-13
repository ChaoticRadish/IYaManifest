using Common_Util.Data.Struct;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Core.Base
{
    public abstract class ManifestFileReaderBase : IManifestFileReader
    {

        public abstract Task<IOperationResult<IManifest<THead, TItem>>> ReadAsync<THead, TItem>()
            where THead : IManifestHead
            where TItem : IManifestItem;


        #region 释放模式
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    DisposeManagedObject();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                DisposeUnmangedObject();
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~ManifestFileReaderBase()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放托管状态(托管对象)
        /// </summary>
        protected virtual void DisposeManagedObject()
        {

        }
        /// <summary>
        /// <para>释放未托管的资源(未托管的对象)并重写终结器</para>
        /// <para>将大型字段设置为 null</para>
        /// </summary>
        protected virtual void DisposeUnmangedObject()
        {

        }
        #endregion
    }
}
