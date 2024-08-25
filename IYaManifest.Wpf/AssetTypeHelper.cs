using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IYaManifest.Wpf
{
    internal static class AssetTypeHelper
    {
        /// <summary>
        /// 判断获取一个资源对象的类型 
        /// <para>可传入 <see cref="Type"/> 或 <see cref="IAsset"/>, 如果传入对象为null, 或者不继承 <see cref="IAsset"/> 则返回 false</para>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Type? GetType(object? value)
        {
            if (value == null) { return null; }
            Type type;
            if (value is Type _t)
            {
                if (_t.IsAssignableTo(typeof(IAsset)))
                {
                    type = _t;
                }
                else return null;
            }
            else if (value is IAsset asset)
            {
                type = asset.GetType();
            }
            else return null;

            return type;
        }
    }
}
