using FormworkOptimize.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FormworkOptimize.App.UI.Converters
{
    public class PlywoodSectionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value)?.ConvertUnderscoreToWhiteSpace().ToLower().ToPascalCase().ConvertZeroToDot().Remove(((string)value).Length - 2, 2) + "mm";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value)?.ConvertWhiteSpaceToUnderscore().ConvertDotToZero().ToUpper();
        }
    }
}
