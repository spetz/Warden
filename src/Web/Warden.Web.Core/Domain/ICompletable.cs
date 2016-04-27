using System;

namespace Warden.Web.Core.Domain
{
    public interface ICompletable
    {
        DateTime CompletedAt { get; }
    }
}