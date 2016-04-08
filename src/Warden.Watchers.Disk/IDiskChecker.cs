using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Warden.Watchers.Disk
{
    public interface IDiskChecker
    {
        Task<DiskCheck> CheckAsync(IEnumerable<string> partitions = null, IEnumerable<string> directories = null,
            IEnumerable<string> files = null);
    }

    public class DiskChecker : IDiskChecker
    {
        public async Task<DiskCheck> CheckAsync(IEnumerable<string> partitions = null,
            IEnumerable<string> directories = null, IEnumerable<string> files = null)
            => DiskCheck.Create(GetFreeSpace(), GetUsedSpace(), CheckPartitions(partitions),
                CheckDirectories(directories), CheckFiles(files));

        private long GetFreeSpace()
        {
            //TODO: implement free space checking
            return 0;
        }

        private long GetUsedSpace()
        {
            //TODO: implement used space checking
            return 0;
        }

        private IEnumerable<PartitionInfo> CheckPartitions(IEnumerable<string> partitions = null)
            => partitions?.Select(CheckPartition) ?? Enumerable.Empty<PartitionInfo>();

        private PartitionInfo CheckPartition(string partition)
        {
            if (string.IsNullOrWhiteSpace(partition))
                return null;

            //TODO: implement partition checking
            return null;
        }

        private IEnumerable<DirectoryInfo> CheckDirectories(IEnumerable<string> directories = null)
            => directories?.Select(CheckDirectory) ?? Enumerable.Empty<DirectoryInfo>();


        private DirectoryInfo CheckDirectory(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                return null;

            //TODO: implement directory checking
            return null;
        }

        private IEnumerable<FileInfo> CheckFiles(IEnumerable<string> files = null)
            => files?.Select(CheckFile) ?? Enumerable.Empty<FileInfo>();

        private FileInfo CheckFile(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
                return null;

            var partition = file.Contains(":") ? file.Split(':').First().ToUpperInvariant() : string.Empty;
            var info = new System.IO.FileInfo(file);
            if (!info.Exists)
                return FileInfo.NotFound(info.Name, info.FullName, info.Extension, partition, info.DirectoryName);

            return FileInfo.Create(info.Name, info.FullName, info.Extension, info.Length, partition,
                info.DirectoryName);
        }
    }
}