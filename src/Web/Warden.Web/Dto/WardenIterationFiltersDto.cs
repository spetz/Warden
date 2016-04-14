using System;

namespace Warden.Web.Dto
{
    public class WardenIterationFiltersDto
    {
        public string WatcherName { get; set; }
        public string WatcherTypeName { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public ResultType ResultType { get; set; }
    }

    public enum ResultType
    {
        All = 0,
        Valid = 1,
        Invalid = 2
    }
}