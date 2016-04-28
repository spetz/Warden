using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Warden.Web.Core.Extensions;
using Warden.Web.Core.Services;

namespace Warden.Web.Hubs
{
    [Authorize]
    public class WardenHub : Hub
    {
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;

        public WardenHub(IUserService userService, IOrganizationService organizationService)
        {
            _userService = userService;
            _organizationService = organizationService;
        }

        public override async Task OnConnected()
        {
            var groupName = await ParseRequestAndGetWardenGroupNameOrFailAsync();
            await Groups.Add(Context.ConnectionId, groupName);
            await base.OnConnected();
        }

        public override async Task OnReconnected()
        {
            var groupName = await ParseRequestAndGetWardenGroupNameOrFailAsync();
            await Groups.Add(Context.ConnectionId, groupName);
            await base.OnReconnected();
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            var groupName = await ParseRequestAndGetWardenGroupNameOrFailAsync();
            await Groups.Remove(Context.ConnectionId, groupName);
            await base.OnDisconnected(stopCalled);
        }

        private async Task<string> ParseRequestAndGetWardenGroupNameOrFailAsync()
        {
            var organizationIdValue = Context.QueryString["organizationId"];
            var wardenName = Context.QueryString["wardenName"];
            if (organizationIdValue.Empty() || wardenName.Empty())
                throw new InvalidOperationException("Empty organization id and/or warden name.");

            Guid organizationId;
            if (!Guid.TryParse(organizationIdValue, out organizationId))
                throw new InvalidOperationException("Invalid organization id.");

            var hasAccess = await _organizationService.IsUserInOrganizationAsync(organizationId,
                Guid.Parse(Context.User.Identity.Name));
            if (!hasAccess)
                throw new InvalidOperationException("No access to the selected organization and warden.");

            return GetWardenGroupName(organizationId, wardenName);
        }

        private static string GetWardenGroupName(Guid organizationId, string wardenName)
            => $"{organizationId}::{wardenName}".TrimToLower();
    }
}