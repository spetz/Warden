using System.Collections.Generic;

namespace Warden.Watchers.Disk
{
    public class DiskCheck
    {
        public double FreeSpace { get; }
        public double UsedSpace { get; }
        public IEnumerable<PartitionInfo> Partitions { get; }
        public IEnumerable<DirectoryInfo> Directories { get; }
        public IEnumerable<FileInfo> Files { get; }

        public DiskCheck(double freeSpace, double usedSpace, 
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
        public double UsedSpace { get; }
        public double FreeSpace { get; }
        public bool Exists { get; }

        public PartitionInfo(string name, double usedSpace, double freeSpace, bool exists)
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
        public double SizeBytes { get; }
        public string Partition { get; }
        public string Directory { get; }

        public FileInfo(string name, string path, string extension, bool exists, double sizeBytes, 
            string partition, string directory)
        {
            Name = name;
            Path = path;
            Extension = extension;
            Exists = exists;
            SizeBytes = sizeBytes;
            Partition = partition;
            Directory = directory;
        }
    }

    public class DirectoryInfo
    {
        public string Name { get; }
        public string Path { get; }
        public int FilesCount { get; }
        public double SizeBytes { get; }
        public bool Exists { get; }

        public DirectoryInfo(string name, string path, int filesCount, double sizeBytes, bool exists)
        {
            Name = name;
            Path = path;
            FilesCount = filesCount;
            SizeBytes = sizeBytes;
            Exists = exists;
        }
    }
}