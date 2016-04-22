using System.ComponentModel.DataAnnotations;

namespace Warden.Web.ViewModels
{
    public class AddWardenToOrganizationViewModel
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        [StringLength(100)]
        public string Name { get; set; }
    }
}