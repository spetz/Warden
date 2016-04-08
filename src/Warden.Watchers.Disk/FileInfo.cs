namespace Warden.Watchers.Disk
{
    /// <summary>
    /// Details of the performed file analysis.
    /// </summary>
    public class FileInfo
    {
        /// <summary>
        /// Name of the file e.g. Image.jpg.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Full path of the file e.g. D:\Images\Image.jpg.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Extension of the file e.g. jpg.
        /// </summary>
        public string Extension { get; }

        /// <summary>
        /// Flag determining whether the file exists.
        /// </summary>
        public bool Exists { get; }

        /// <summary>
        /// Size in bytes.
        /// </summary>
        public long Size { get; }

        /// <summary>
        /// Name of the partition in which the file exists/should exist e.g. D:\.
        /// </summary>
        public string Partition { get; }

        /// <summary>
        /// Full path of the directory in which the file exists/should exist e.g. D:\Images.
        /// </summary>
        public string Directory { get; }

        protected FileInfo(string path, string name, string extension,
            bool exists, long size, string partition, string directory)
        {
            Path = path;
            Name = name;
            Partition = partition;
            Extension = extension;
            Exists = exists;
            Size = size;
            Partition = partition;
            Directory = directory;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the file e.g. Image.jpg.</param>
        /// <param name="path">Full path of the file e.g. D:\Images\Image.jpg.</param>
        /// <param name="extension">Extension of the file e.g. jpg.</param>
        /// <param name="partition">Name of the partition in which the file should exist e.g. D:\.</param>
        /// <param name="directory">Full path of the directory in which the file exists e.g. D:\Images.</param>
        /// <returns></returns>
        public static FileInfo NotFound(string name, string path, string extension, string partition, string directory)
            => new FileInfo(name, path, extension, false, 0, partition, directory);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the file e.g. Image.jpg.</param>
        /// <param name="path">Full path of the file e.g. D:\Images\Image.jpg.</param>
        /// <param name="extension">Extension of the file e.g. jpg.</param>
        /// <param name="sizeBytes">Size in bytes.</param>
        /// <param name="partition">Name of the directory in which the file exists e.g. D:\.</param>
        /// <param name="directory">Full path of the directory in which the file exists e.g. D:\Images.</param>
        /// <returns></returns>
        public static FileInfo Create(string name, string path, string extension, long sizeBytes,
            string partition, string directory)
            => new FileInfo(name, path, extension, true, sizeBytes, partition, directory);
    }
}