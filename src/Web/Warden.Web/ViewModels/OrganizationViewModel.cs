using System.ComponentModel.DataAnnotations;
using System.Linq;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Dto;

namespace Warden.Web.ViewModels
{
    public class OrganizationViewModel
    {
        public OrganizationDto Organization { get; set; }
        public UserInOrganizationDto Owner => Organization.Users.First(x => x.Role == OrganizationRole.Owner);
    }
}