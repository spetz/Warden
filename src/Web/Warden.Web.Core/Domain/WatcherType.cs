﻿namespace Warden.Web.Core.Domain
{
    public enum WatcherType
    {
        Custom = 0,
        Disk = 1,
        MongoDb = 2,
        MsSql = 3,
        Performance = 4,
        Process = 5,
        Redis = 6,
        Web = 7
    }
}