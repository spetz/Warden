namespace Warden.Watchers.Process
{
    /// <summary>
    /// Details of the process;
    /// </summary>
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

        /// <summary>
        /// Factory method for creating process details
        /// </summary>
        /// <param name="id">Unique identifier of the process.</param>
        /// <param name="name">Name of the process.</param>
        /// <param name="exists">Flag determining whether the process exists.</param>
        /// <param name="state">State of the process.</param>
        /// <returns>Instance of DirectoryInfo.</returns>
        public static ProcessInfo Create(int id, string name, bool exists, ProcessState state)
            => new ProcessInfo(id, name, exists, state);

        /// <summary>
        /// Factory method for creating empty process details.
        /// </summary>
        public static ProcessInfo Empty => new ProcessInfo();
    }
}