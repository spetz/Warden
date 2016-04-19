using System;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Dto
{
    public class WardenCheckResultDto
    {
        public bool IsValid { get; set; }
        public WatcherCheckResultDto WatcherCheckResult { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime CompletedAt { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public ExceptionDto Exception { get; set; }

        public WardenCheckResultDto()
        {
        }

        public WardenCheckResultDto(WardenCheckResult result)
        {
            IsValid = result.IsValid;
            WatcherCheckResult = new WatcherCheckResultDto(result.WatcherCheckResult);
            StartedAt = result.StartedAt;
            CompletedAt = result.CompletedAt;
            ExecutionTime = result.ExecutionTime;
            Exception = result.Exception == null ? null : new ExceptionDto(result.Exception);
        }
    }
}