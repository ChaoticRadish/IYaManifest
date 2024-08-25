using IYaManifest.Base.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifestDemo.Assets
{
    public interface ITestTextAsset : IDataStringAsset
    {
        string Data { get; set; }
    }
}
