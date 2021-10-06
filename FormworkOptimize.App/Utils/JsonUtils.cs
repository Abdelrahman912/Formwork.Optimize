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
        private static string Read(this string filePath)
        {
            string jsonFromFile;
            using (var reader = new StreamReader(filePath))
            {
                jsonFromFile = reader.ReadToEnd();
            }
            return jsonFromFile;
        }

        public static Validation<List<T>> ReadAsJsonList<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return FileNotFound(filePath);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var collection =  JsonConvert.DeserializeObject<List<T>>(filePath.Read(),settings);
            return collection; //TODO: Complete Validation.
        }
        public static Validation<T> ReadAsJsonObject<T>(string filePath)
        {
            if (!File.Exists(filePath))
                return FileNotFound(filePath);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var obj =  JsonConvert.DeserializeObject<T>(filePath.Read(),settings);
            return obj;//TODO: Complete Validation
        }

        public static async Task<Exceptional<Unit>> WriteAsJson<T>(this T data, string filePath)
        {
            try
            {
                var jsonToWrite = JsonConvert.SerializeObject(data, Formatting.Indented);
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
