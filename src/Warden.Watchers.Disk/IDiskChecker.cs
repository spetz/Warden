using System.Collections.Generic;
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
        {
            //Simple mock for now
            return new DiskCheck(1, 1, new List<PartitionInfo>(), new List<DirectoryInfo>(), new List<FileInfo>());
        }
    }
}