using System;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Dto
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public Role Role { get; set; }
        public Guid RecentlyViewedOrganizationId { get; set; }
        public Guid RecentlyViewedWardenId { get; set; }

        public UserDto()
        {
        }

        public UserDto(User user)
        {
            Id = user.Id;
            Email = user.Email;
            Role = user.Role;
            RecentlyViewedOrganizationId = user.RecentlyViewedOrganizationId;
            RecentlyViewedWardenId = user.RecentlyViewedWardenId;
        }
    }
}