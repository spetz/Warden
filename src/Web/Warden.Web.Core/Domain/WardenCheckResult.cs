using System;

namespace Warden.Web.Core.Domain
{
    public class WardenCheckResult
    {
        public bool IsValid { get; protected set; }
        public WatcherCheckResult WatcherCheckResult { get; protected set; }
        public DateTime StartedAt { get; protected set; }
        public DateTime CompletedAt { get; protected set; }
        public TimeSpan ExecutionTime { get; protected set; }
        public ExceptionInfo Exception { get; protected set; }
    }
}