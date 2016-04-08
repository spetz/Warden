using System.Collections.Generic;

namespace Warden.Watchers.Disk
{
    public class DiskCheck
    {
        public long FreeSpace { get; }
        public long UsedSpace { get; }
        public IEnumerable<PartitionInfo> Partitions { get; }
        public IEnumerable<DirectoryInfo> Directories { get; }
        public IEnumerable<FileInfo> Files { get; }

        public DiskCheck(long freeSpace, long usedSpace, 
            IEnumerable<PartitionInfo> partitions, 
            IEnumerable<DirectoryInfo> directories,
            IEnumerable<FileInfo> files)
        {
            FreeSpace = freeSpace;
            UsedSpace = usedSpace;
            Partitions = partitions;
            Directories = directories;
            Files = files;
        }
    }

    public class PartitionInfo
    {
        public string Name { get; }
        public long UsedSpace { get; }
        public long FreeSpace { get; }
        public bool Exists { get; }

        public PartitionInfo(string name, long usedSpace, long freeSpace, bool exists)
        {
            Name = name;
            UsedSpace = usedSpace;
            FreeSpace = freeSpace;
            Exists = exists;
        }
    }

    public class FileInfo
    {
        public string Name { get; }
        public string Path { get; }
        public string Extension { get; }
        public bool Exists { get; }
        public long SizeBytes { get; }
        public string Partition { get; }
        public string Directory { get; }

        protected FileInfo(string path, string name, string extension,
            bool exists, long sizeBytes, string partition, string directory)
        {
            Path = path;
            Name = name;
            Partition = partition;
            Extension = extension;
            Exists = exists;
            SizeBytes = sizeBytes;
            Partition = partition;
            Directory = directory;
        }

        public static FileInfo NotFound(string path, string name, string extension, string partition, string directory)
            => new FileInfo(path, name, extension, false, 0, partition, directory);

        public static FileInfo Create(string path, string name, string extension, long sizeBytes,
            string partition, string directory)
            => new FileInfo(path, name, extension, true, sizeBytes, partition, directory);
    }

    public class DirectoryInfo
    {
        public string Name { get; }
        public string Path { get; }
        public int FilesCount { get; }
        public long SizeBytes { get; }
        public bool Exists { get; }

        public DirectoryInfo(string name, string path, int filesCount, long sizeBytes, bool exists)
        {
            Name = name;
            Path = path;
            FilesCount = filesCount;
            SizeBytes = sizeBytes;
            Exists = exists;
        }
    }
}