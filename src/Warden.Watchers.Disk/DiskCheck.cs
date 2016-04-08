using System.Collections.Generic;

namespace Warden.Watchers.Disk
{
    /// <summary>
    /// Details of the performed disk analysis.
    /// </summary>
    public class DiskCheck
    {
        /// <summary>
        /// Total free space in bytes available on disk.
        /// </summary>
        public long FreeSpace { get; }

        /// <summary>
        /// Total used space in bytes on disk.
        /// </summary>
        public long UsedSpace { get; }

        /// <summary>
        /// Total space in bytes on disk.
        /// </summary>
        public long TotalSpace => UsedSpace + FreeSpace;

        /// <summary>
        /// Collection of partitions that have been checked.
        /// </summary>
        public IEnumerable<PartitionInfo> Partitions { get; }

        /// <summary>
        /// Collection of directories that have been checked.
        /// </summary>
        public IEnumerable<DirectoryInfo> Directories { get; }

        /// <summary>
        /// Collection of files that have been checked.
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="freeSpace">Total free space in bytes available on disk.</param>
        /// <param name="usedSpace">Total used space in bytes on disk.</param>
        /// <param name="partitions">Collection of partitions that have been checked.</param>
        /// <param name="directories">Collection of directories that have been checked.</param>
        /// <param name="files">Collection of files that have been checked.</param>
        /// <returns></returns>
        public static DiskCheck Create(long freeSpace, long usedSpace,
            IEnumerable<PartitionInfo> partitions,
            IEnumerable<DirectoryInfo> directories,
            IEnumerable<FileInfo> files)
            => new DiskCheck(freeSpace, usedSpace, partitions, directories, files);
    }
}