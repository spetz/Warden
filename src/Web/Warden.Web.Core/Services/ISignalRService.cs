using System;
using Microsoft.AspNet.SignalR;
using Warden.Web.Core.Dto;
using Warden.Web.Core.Extensions;

namespace Warden.Web.Core.Services
{
    public interface ISignalRService
    {
        void SendIterationCreated(Guid organizationId, WardenIterationDto iteration);
    }

    public class SignalRService : ISignalRService
    {
        private readonly IHubContext _hub;

        public SignalRService(IHubContext hub)
        {
            _hub = hub;
        }

        public void SendIterationCreated(Guid organizationId, WardenIterationDto iteration)
        {
            var groupName = GetWardenGroupName(organizationId, iteration.WardenName);
            _hub.Clients.Group(groupName).iterationCreated(iteration);
        }

        private static string GetWardenGroupName(Guid organizationId, string wardenName)
            => $"{organizationId}::{wardenName}".TrimToLower();
    }
}