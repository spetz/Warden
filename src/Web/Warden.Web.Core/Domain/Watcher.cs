using Warden.Web.Core.Extensions;

namespace Warden.Web.Core.Domain
{
    public class Watcher
    {
        public string Name { get; protected set; }
        public WatcherType Type { get; protected set; }
        public string Group { get; protected set; }

        protected Watcher()
        {
        }

        protected Watcher(string name, WatcherType type, string group)
        {
            if (name.Empty())
                throw new DomainException("Watcher name not be empty.");

            Name = name;
            Type = type;
            Group = group;
        }

        public static Watcher Create(string name, WatcherType type, string group = null)
            => new Watcher(name, type, group);
    }
}