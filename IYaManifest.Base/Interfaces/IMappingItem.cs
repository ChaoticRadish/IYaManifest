using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Interfaces
{
    public interface IMappingItem
    {
        public string AssetType { get; }
        public Type AssetClass { get; }
        public Type WriteReadImplClass { get; }
    }
}
