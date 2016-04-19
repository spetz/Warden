using System;
using System.Collections.Generic;

namespace Warden.Web.Core.Domain
{
    public class WardenIteration : Entity, ITimestampable
    {
        private HashSet<WardenCheckResult> _results = new HashSet<WardenCheckResult>();

        public Guid WardenInstanceId { get; protected set; }
        public long Ordinal { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime StartedAt { get; protected set; }
        public DateTime CompletedAt { get; protected set; }
        public TimeSpan ExecutionTime { get; protected set; }
        public bool IsValid { get; protected set; }

        public IEnumerable<WardenCheckResult> Results
        {
            get { return _results; }
            protected set { _results = new HashSet<WardenCheckResult>(value); }
        }
    }
}