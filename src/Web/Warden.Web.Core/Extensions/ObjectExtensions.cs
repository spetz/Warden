using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Warden.Web.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatString = "yyyy-MM-dd H:mm:ss",
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Populate,
            NullValueHandling = NullValueHandling.Include,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter
                {
                    AllowIntegerValues = true,
                    CamelCaseText = true
                }
            }
        };

        public static string ToJson<T>(this T value, JsonSerializerSettings serializerSettings = null)
        {
            return JsonConvert.SerializeObject(value, serializerSettings ?? DefaultJsonSerializerSettings);
        }

        public static T FromJson<T>(this string value, JsonSerializerSettings serializerSettings = null)
        {
            return JsonConvert.DeserializeObject<T>(value, serializerSettings ?? DefaultJsonSerializerSettings);
        }
    }
}