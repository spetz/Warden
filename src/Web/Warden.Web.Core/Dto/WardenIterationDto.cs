using System;
using System.Collections.Generic;
using System.Linq;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Dto
{
    public class WardenIterationDto
    {
        public Guid Id { get; set; }
        public string WardenName { get; set; }
        public long Ordinal { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public bool IsValid { get; set; }
        public IEnumerable<WardenCheckResultDto> Results { get; set; }

        public WardenIterationDto()
        {
        }

        public WardenIterationDto(WardenIteration iteration)
        {
            Id = iteration.Id;
            WardenName = iteration.Warden.Name;
            Ordinal = iteration.Ordinal;
            StartedAt = iteration.StartedAt;
            CompletedAt = iteration.CompletedAt;
            ExecutionTime = iteration.ExecutionTime;
            IsValid = iteration.IsValid;
            Results = iteration.Results.Select(x => new WardenCheckResultDto(x)
            {
                IterationId = iteration.Id
            });
        }
    }
}