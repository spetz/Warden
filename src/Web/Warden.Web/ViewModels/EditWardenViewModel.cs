using System.ComponentModel.DataAnnotations;

namespace Warden.Web.ViewModels
{
    public class EditWardenViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        [StringLength(100, ErrorMessage = "Name can not have more than 100 characters.")]
        public string Name { get; set; }
    }
}