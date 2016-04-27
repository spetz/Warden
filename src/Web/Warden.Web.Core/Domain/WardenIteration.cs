using System;
using System.Collections.Generic;
using System.Linq;

namespace Warden.Web.Core.Domain
{
    public class WardenIteration : Entity, ITimestampable, ICompletable, IValidatable
    {
        private HashSet<WardenCheckResult> _results = new HashSet<WardenCheckResult>();

        public WardenInfo Warden { get; protected set; }
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

        protected WardenIteration()
        {
        }

        public WardenIteration(string wardenName, Organization organization, long ordinal,
            DateTime startedAt, DateTime completedAt, bool isValid)
        {
            Warden = WardenInfo.Create(wardenName, organization);
            Ordinal = ordinal;
            StartedAt = startedAt;
            CompletedAt = completedAt;
            IsValid = isValid;
            CreatedAt = DateTime.Now;
        }

        public void AddResult(WardenCheckResult result)
        {
            if (result == null)
                throw new DomainException("Can not add null warden check result.");

            if (Results.Contains(result))
                return;

            _results.Add(result);
        }
    }
}