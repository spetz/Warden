namespace Warden.Web.Core.Domain
{
    public class WatcherCheckResult
    {
        public Watcher Watcher { get; protected set; }
        public string Description { get; protected set; }
        public bool IsValid { get; protected set; }

        protected WatcherCheckResult()
        {
        }

        protected WatcherCheckResult(Watcher watcher, string description, bool isValid)
        {
            if(watcher == null)
                throw new DomainException("Watcher can not be null.");

            Watcher = watcher;
            Description = description;
            IsValid = isValid;
        }

        public static WatcherCheckResult Valid(Watcher watcher, string description)
            => new WatcherCheckResult(watcher, description, true);

        public static WatcherCheckResult Invalid(Watcher watcher, string description)
            => new WatcherCheckResult(watcher, description, false);
    }
}