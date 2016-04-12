namespace Warden.Watchers.Disk
{
    /// <summary>
    /// Details of the performed partition analysis.
    /// </summary>
    public class PartitionInfo
    {
        /// <summary>
        /// Name of the partition e.g. C:\.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Total used space in bytes on disk by partition.
        /// </summary>
        public long UsedSpace { get; }

        /// <summary>
        /// Total free space in bytes available on partition.
        /// </summary>
        public long FreeSpace { get; }

        /// <summary>
        /// Total space in bytes on partition.
        /// </summary>
        public long TotalSpace => UsedSpace + FreeSpace;

        /// <summary>
        /// Flag determining whether the partition exists.
        /// </summary>
        public bool Exists { get; }

        public PartitionInfo(string name, long usedSpace, long freeSpace, bool exists)
        {
            Name = name;
            UsedSpace = usedSpace;
            FreeSpace = freeSpace;
            Exists = exists;
        }

        /// <summary>
        /// Factory method for creating a new instance of PartitionInfo for the non-existing partition.
        /// </summary>
        /// <param name="name">Name of the partition e.g. C:\.</param>
        /// <returns>Instance of PartitionInfo.</returns>
        public static PartitionInfo NotFound(string name)
            => new PartitionInfo(name, 0, 0, false);

        /// <summary>
        /// Factory method for creating a new instance of PartitionInfo for the existing partition.
        /// </summary>
        /// <param name="name">Name of the partition e.g. C:\.</param>
        /// <param name="usedSpace">Total used space in bytes on disk by partition.</param>
        /// <param name="freeSpace">Total free space in bytes available on disk.</param>
        /// <returns>Instance of PartitionInfo.</returns>
        public static PartitionInfo Create(string name, long usedSpace, long freeSpace)
            => new PartitionInfo(name, usedSpace, freeSpace, true);
    }
}