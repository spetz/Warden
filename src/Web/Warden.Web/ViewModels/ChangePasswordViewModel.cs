using System.ComponentModel.DataAnnotations;

namespace Warden.Web.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Actual password is required.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Actual password must contain between 4-100 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Actual password")]
        public string ActualPassword { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "New password must contain between 4-100 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "New password and its confirmation do not match.")]
        [Display(Name = "Confirm new password")]
        public string ConfirmNewPassword { get; set; }
    }
}