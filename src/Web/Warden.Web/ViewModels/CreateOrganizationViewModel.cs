using System.ComponentModel.DataAnnotations;

namespace Warden.Web.ViewModels
{
    public class CreateOrganizationViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Name")]
        [StringLength(100)]
        public string Name { get; set; }
    }
}