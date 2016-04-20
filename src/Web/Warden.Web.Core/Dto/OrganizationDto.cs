using System;
using Warden.Web.Core.Domain;

namespace Warden.Web.Core.Dto
{
    public class OrganizationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public OrganizationDto()
        {
        }

        public OrganizationDto(Organization organization)
        {
            Id = organization.Id;
            Name = organization.Name;
        }
    }
}