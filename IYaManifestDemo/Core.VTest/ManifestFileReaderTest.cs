using Common_Util.Data.Struct;
using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifestDemo.Core.VTest
{
    internal class ManifestFileReaderTest : IManifestFileReader<ManifestHead, ManifestItem>
    {
        public void Dispose()
        {
        }

        public Task<IOperationResult<IManifest<THead, TItem>>> ReadAsync<THead, TItem>()
            where THead : IManifestHead
            where TItem : IManifestItem
        {
            this.CheckTypeThrowException(typeof(THead), typeof(TItem));

            throw new NotImplementedException();

        }
    }
}
