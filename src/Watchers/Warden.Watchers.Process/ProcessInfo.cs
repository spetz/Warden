namespace Warden.Watchers.Process
{
    public class ProcessInfo
    {
        /// <summary>
        /// Unique identifier of the process.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Name of the process.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Flag determining whether the process exists.
        /// </summary>
        public bool Exists { get; }

        /// <summary>
        /// State of the process.
        /// </summary>
        public ProcessState State { get; }

        protected ProcessInfo()
        {
        }

        protected ProcessInfo(int id, string name, bool exists, ProcessState state)
        {
            Id = id;
            Name = name;
            Exists = exists;
            State = state;
        }

        public static ProcessInfo Create(int id, string name, bool exists, ProcessState state)
            => new ProcessInfo(id, name, exists, state);

        public static ProcessInfo Empty => new ProcessInfo();
    }
}