using FormworkOptimize.Core.Extensions;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FormworkOptimize.App.UI.Converters
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var myEnum = value as Enum;
            var result =  myEnum.GetDescription();
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
