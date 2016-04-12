namespace Warden.Watchers.Disk
{
    /// <summary>
    /// Details of the performed directory analysis.
    /// </summary>
    public class DirectoryInfo
    {
        /// <summary>
        /// Name of the directory e.g. Images.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Full path of the directory e.g. D:\Images.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Number of files existing in the directory.
        /// </summary>
        public int FilesCount { get; }

        /// <summary>
        /// Size in bytes.
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// Flag determining whether the directory exists.
        /// </summary>
        public bool Exists { get; }

        public DirectoryInfo(string name, string path, int filesCount, long size, bool exists)
        {
            Name = name;
            Path = path;
            FilesCount = filesCount;
            Size = size;
            Exists = exists;
        }

        /// <summary>
        /// Factory method for creating a new instance of DirectoryInfo for the non-existing directory.
        /// </summary>
        /// <param name="name">Name of the directory e.g. Images</param>
        /// <param name="path">Full path of the directory e.g. D:\Images</param>
        /// <returns>Instance of DirectoryInfo.</returns>
        public static DirectoryInfo NotFound(string name, string path)
            => new DirectoryInfo(path, name, 0, 0, false);

        /// <summary>
        /// Factory method for creating a new instance of DirectoryInfo for the existing directory.
        /// </summary>
        /// <param name="name">Name of the directory e.g. Images</param>
        /// <param name="path">Full path of the directory e.g. D:\Images</param>
        /// <param name="filesCount">Number of files existing in the directory.</param>
        /// <param name="sizeBytes">Size in bytes.</param>
        /// <returns>Instance of DirectoryInfo.</returns>
        public static DirectoryInfo Create(string name, string path, int filesCount, long sizeBytes)
            => new DirectoryInfo(path, name, filesCount, sizeBytes, true);
    }
}