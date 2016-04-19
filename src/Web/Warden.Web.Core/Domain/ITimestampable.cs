using System;

namespace Warden.Web.Core.Domain
{
    public interface ITimestampable
    {
        DateTime CreatedAt { get; }
    }
}