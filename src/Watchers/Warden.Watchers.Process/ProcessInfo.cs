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
        /// Flag determining whether the process is responding. 
        /// Currently it's always true as it's a still missing property in .NET Core.
        /// </summary>
        public bool Responding { get; }

        protected ProcessInfo()
        {
        }

        protected ProcessInfo(int id, string name, bool exists, bool responding)
        {
            Id = id;
            Name = name;
            Exists = exists;
            Responding = responding;
        }

        /// <summary>
        /// Factory method for creating process details
        /// </summary>
        /// <param name="id">Unique identifier of the process.</param>
        /// <param name="name">Name of the process.</param>
        /// <param name="exists">Flag determining whether the process exists.</param>
        /// <param name="responding">Flag determining whether the process is responding.</param>
        /// <returns>Instance of DirectoryInfo.</returns>
        public static ProcessInfo Create(int id, string name, bool exists, bool responding)
            => new ProcessInfo(id, name, exists, responding);

        /// <summary>
        /// Factory method for creating empty process details.
        /// </summary>
        public static ProcessInfo Empty => new ProcessInfo();
    }
}