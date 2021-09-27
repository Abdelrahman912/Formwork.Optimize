using CSharp.Functional.Constructs;
using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using static FormworkOptimize.Core.Errors.Errors;
using static CSharp.Functional.Extensions.ValidationExtension;
using System.Linq;

namespace FormworkOptimize.App.Utils
{
    public static class CsvUtils
    {
        public static Validation<List<T>> ReadAsCsv<T>(this string filePath)
        {
            if (!File.Exists(filePath))
                return FileNotFound(filePath);

            try
            {
                using (var streamReader = new StreamReader(filePath))
                using (var csvRedaer = new CsvReader(streamReader, CultureInfo.InvariantCulture))
                {
                    return Valid(csvRedaer.GetRecords<T>().ToList());
                }
            }
            catch (IOException)
            {
               return FileUsedByAnotherProcess(filePath);
            }
        }

        public static async Task<Exceptional<string>> WriteAsCsv<T>(this IEnumerable<T> list, string directory, string fileName)
        {
            try
            {
                string csvFilePath = $"{directory}\\{fileName}.csv";

                using (var writer = new StreamWriter(csvFilePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(list);
                }

                return $"{fileName}.csv";
            }
            catch (System.Exception e)
            {
                return e;
            }

        }
    }
}
