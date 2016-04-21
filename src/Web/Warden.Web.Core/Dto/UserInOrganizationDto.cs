using System;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Dto
{
    public class UserInOrganizationDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public OrganizationRole Role { get; set; }
        public DateTime CreatedAt { get; set; }

        public UserInOrganizationDto()
        {
        }

        public UserInOrganizationDto(UserInOrganization user)
        {
            Id = user.Id;
            Email = user.Email;
            Role = user.Role;
            CreatedAt = user.CreatedAt;
        }
    }
}