using System.ComponentModel.DataAnnotations;

namespace MediCareConnect.Models
{
    public class CreatePatientViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Email (optional)")]
        [EmailAddress]
        public string? Email { get; set; }
    }
}
