using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FormworkOptimize.Core.Extensions
{
    public static class StringExtension
    {
        // Convert the underscore in a string to white space.
        public static string ConvertUnderscoreToWhiteSpace(this string the_string)
        {
            string result = the_string.Replace('_', ' ');
            return result;
        }

        // Convert the white space in a string to underscore.
        public static string ConvertWhiteSpaceToUnderscore(this string the_string)
        {
            string result = the_string.Replace(' ', '_');
            return result;
        }

        // Convert the string to Pascal case.
        public static string ToPascalCase(this string the_string)
        {
            TextInfo info = Thread.CurrentThread.CurrentCulture.TextInfo;
            string result = info.ToTitleCase(the_string);
            return result;
        }

        // Convert the zero '0' in a string to dot '.'.
        public static string ConvertZeroToDot(this string the_string)
        {
            string result = the_string.Replace('0', '.');
            return result;
        }

        // Convert the dot '.' in a string to zero '0'.
        public static string ConvertDotToZero(this string the_string)
        {
            string result = the_string.Replace('.', '0');
            return result;
        }
    }
}
