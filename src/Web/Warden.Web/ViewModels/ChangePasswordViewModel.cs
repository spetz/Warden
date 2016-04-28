using System.ComponentModel.DataAnnotations;

namespace Warden.Web.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "ActualPassword")]
        public string ActualPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        [Display(Name = "ConfirmNewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}