using System;

namespace Warden.Web.Core.Domain
{
    public interface IIdentifiable
    {
        Guid Id { get; }
    }
}