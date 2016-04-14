using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Warden.Web.Dto
{
    public class WatcherCheckResultDto
    {
        public string WatcherName { get; set; }

        [JsonProperty("WatcherType")]
        [BsonIgnore]
        public string WatcherTypeFullName { get; set; }

        [JsonIgnore]
        public string WatcherType { get; set; }

        public string Description { get; set; }
        public bool IsValid { get; set; }
    }
}