using System;
using System.Collections.Generic;

namespace Warden.Web.Dto
{
    public class WardenIterationDto
    {
        public long Ordinal { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public bool IsValid { get; set; }
        public IEnumerable<WardenCheckResultDto> Results { get; set; }
    }
}