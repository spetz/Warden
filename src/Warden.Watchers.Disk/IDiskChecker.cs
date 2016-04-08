using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Warden.Watchers.Disk
{
    /// <summary>
    /// Custom disk checker for disk analysis.
    /// </summary>
    public interface IDiskChecker
    {
        Task<DiskCheck> CheckAsync(IEnumerable<string> partitions = null,
            IEnumerable<string> directories = null, IEnumerable<string> files = null);
    }

    /// <summary>
    /// Default implementation of the IDiskChecker based on System.IO.
    /// </summary>
    public class DiskChecker : IDiskChecker
    {
        public async Task<DiskCheck> CheckAsync(IEnumerable<string> partitions = null,
            IEnumerable<string> directories = null, IEnumerable<string> files = null)
            => await Task.Factory.StartNew(() => DiskCheck.Create(GetFreeSpace(), GetUsedSpace(),
                CheckPartitions(partitions), CheckDirectories(directories), CheckFiles(files)));

        private static long GetFreeSpace()
            => DriveInfo.GetDrives().Sum(x => x.TotalFreeSpace);

        private static long GetUsedSpace()
            => DriveInfo.GetDrives().Sum(x => x.TotalSize) - GetFreeSpace();

        private IEnumerable<PartitionInfo> CheckPartitions(IEnumerable<string> partitions = null)
            => partitions?.Select(CheckPartition) ?? Enumerable.Empty<PartitionInfo>();

        private PartitionInfo CheckPartition(string partition)
        {
            if (string.IsNullOrWhiteSpace(partition))
                return null;

            if (partition.EndsWith(":"))
                partition += @"\";
            else if (!partition.EndsWith(@":\"))
                partition += @":\";

            var name = partition.ToUpperInvariant();
            var drives = DriveInfo.GetDrives();
            var info = drives.FirstOrDefault(x => x.Name.Equals(name));

            return info == null
                ? PartitionInfo.NotFound(name)
                : PartitionInfo.Create(info.Name, info.TotalSize - info.TotalFreeSpace, info.TotalFreeSpace);
        }

        private IEnumerable<DirectoryInfo> CheckDirectories(IEnumerable<string> directories = null)
            => directories?.Select(CheckDirectory) ?? Enumerable.Empty<DirectoryInfo>();

        private DirectoryInfo CheckDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                return null;

            var info = new System.IO.DirectoryInfo(directory);
            if (!info.Exists)
                return DirectoryInfo.NotFound(info.Name, info.FullName);

            var files = info.GetFiles();

            return DirectoryInfo.Create(info.Name, info.FullName, files.Length, files.Sum(x => x.Length));
        }

        private IEnumerable<FileInfo> CheckFiles(IEnumerable<string> files = null)
            => files?.Select(CheckFile) ?? Enumerable.Empty<FileInfo>();

        private FileInfo CheckFile(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                return null;

            var partition = file.Contains(":") ? $@"{file.Split(':').First().ToUpperInvariant()}:\" : string.Empty;
            var info = new System.IO.FileInfo(file);
            if (!info.Exists)
                return FileInfo.NotFound(info.Name, info.FullName, info.Extension, partition, info.DirectoryName);

            return FileInfo.Create(info.Name, info.FullName, info.Extension, info.Length, partition,
                info.DirectoryName);
        }
    }
}