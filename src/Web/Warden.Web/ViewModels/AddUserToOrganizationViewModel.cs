using System.ComponentModel.DataAnnotations;

namespace Warden.Web.ViewModels
{
    public class AddUserToOrganizationViewModel
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}