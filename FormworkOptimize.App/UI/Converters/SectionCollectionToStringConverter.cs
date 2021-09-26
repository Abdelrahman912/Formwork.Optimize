using FormworkOptimize.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FormworkOptimize.App.UI.Converters
{
    public class SectionCollectionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Collection<string> result = new Collection<string>();

            foreach (var item in (Collection<string>)value)
            {
                result.Add(item.ConvertUnderscoreToWhiteSpace().ToLower().ToPascalCase());
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Collection<string> result = new Collection<string>();

            foreach (var item in (Collection<string>)value)
            {
                result.Add(item.ConvertWhiteSpaceToUnderscore().ToUpper());
            }

            return result;
        }
    }
}
