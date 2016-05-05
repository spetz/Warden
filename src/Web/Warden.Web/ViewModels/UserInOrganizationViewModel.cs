using System;
using Warden.Web.Core.Dto;

namespace Warden.Web.ViewModels
{
    public class UserInOrganizationViewModel
    {
        public Guid OrganizationId { get; set; }
        public Guid OwnerId { get; set; }
        public UserInOrganizationDto User { get; set; }
    }
}