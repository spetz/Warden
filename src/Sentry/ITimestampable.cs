using System;

namespace Sentry
{
    public interface ITimestampable
    {
        DateTime StartedAt { get; }
        DateTime CompletedAt { get; }
        TimeSpan ExecutionTime { get; }
    }
}