using IYaManifest.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace IYaManifest.Wpf.Converter
{
    /// <summary>
    /// 判断一个类型或对象是否有页面类型映射的转换器
    /// <para>如果为 null, 会返回 false</para>
    /// <para>该类型或对象, 如果不实现接口 <see cref="IAsset"/> 也会直接返回 false </para>
    /// </summary>
    public class HasPageTypeMappingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type? type = AssetTypeHelper.GetType(value);
            if (type == null) return false;

            return PageTypeMapManager.Instance.Exist(type);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
