﻿using FormworkOptimize.Core.Extensions;
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
    public class SectionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value)?.ConvertUnderscoreToWhiteSpace().ToLower().ToPascalCase();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value)?.ConvertWhiteSpaceToUnderscore().ToUpper();
        }
    }
}
