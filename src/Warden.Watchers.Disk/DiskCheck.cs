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

        protected DiskCheck(long freeSpace, long usedSpace, 
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

        public static DiskCheck Create(long freeSpace, long usedSpace,
            IEnumerable<PartitionInfo> partitions,
            IEnumerable<DirectoryInfo> directories,
            IEnumerable<FileInfo> files)
            => new DiskCheck(freeSpace, usedSpace, partitions, directories, files);
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

        public static PartitionInfo NotFound(string name)
            => new PartitionInfo(name, 0, 0, false);

        public static PartitionInfo Create(string name, long usedSpace, long freeSpace)
            => new PartitionInfo(name, usedSpace, freeSpace, true);
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

        public static FileInfo NotFound(string name, string path, string extension, string partition, string directory)
            => new FileInfo(name, path, extension, false, 0, partition, directory);

        public static FileInfo Create(string path, string name, string extension, long sizeBytes,
            string partition, string directory)
            => new FileInfo(name, path, extension, true, sizeBytes, partition, directory);
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

        public static DirectoryInfo NotFound(string name, string path)
            => new DirectoryInfo(path, name, 0, 0, false);

        public static DirectoryInfo Create(string name, string path, int filesCount, long sizeBytes)
            => new DirectoryInfo(path, name, filesCount, sizeBytes, true);
    }
}