using System;

namespace Sentry
{
    public interface ITimestampable
    {
        DateTime StartedAtUtc { get; }
        DateTime CompletedAtUtc { get; }
        TimeSpan ExecutionTime { get; }
    }
}