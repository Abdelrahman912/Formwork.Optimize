using CSharp.Functional.Constructs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using static FormworkOptimize.Core.Errors.Errors;
using Unit = System.ValueTuple;
using static CSharp.Functional.Functional;
using System.Threading.Tasks;

namespace FormworkOptimize.App.Utils
{
    public static class JsonUtils
    {
        private static async Task<string> ReadAsync(this string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static async Task<Validation<List<T>>> ReadAsJsonList<T>(this string filePath)
        {
            if (!File.Exists(filePath))
                return FileNotFound(filePath);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            };
            var jsonAsString = await filePath.ReadAsync();
            var collection =  JsonConvert.DeserializeObject<List<T>>(jsonAsString ,settings);
            return collection; //TODO: Complete Validation.
        }
        public static async Task<Validation<T>> ReadAsJsonObject<T>(this string filePath)
        {
            if (!File.Exists(filePath))
                return FileNotFound(filePath);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            };
            var jsonAsString = await filePath.ReadAsync();
            var obj =  JsonConvert.DeserializeObject<T>(jsonAsString,settings);
            return obj;//TODO: Complete Validation
        }

        public static async Task<Exceptional<Unit>> WriteAsJson<T>(this T data, string filePath)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                };
                var jsonToWrite = JsonConvert.SerializeObject(data, Formatting.Indented,settings);
                using (var writer = new StreamWriter(filePath))
                {
                  await  writer.WriteAsync(jsonToWrite);
                }
                return Unit();
            }
            catch (System.Exception e)
            {
                return e;
            }
           
        }

    }
}
