using System.ComponentModel.DataAnnotations;

namespace Warden.Web.ViewModels
{
    public class AddUserToOrganizationViewModel
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        [StringLength(100)]
        public string Email { get; set; }
    }
}