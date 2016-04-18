using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Warden.Web.Core.Dto
{
    [BsonIgnoreExtraElements]
    public class WardenIterationDto
    {
        public string WardenName { get; set; }
        public long Ordinal { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public bool IsValid { get; set; }
        public IEnumerable<WardenCheckResultDto> Results { get; set; }
    }
}