using System.ComponentModel.DataAnnotations;

namespace MediCareConnect.Models
{
    public class EditDoctorViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Display(Name = "Specialty")]
        public string? Specialty { get; set; }

        [Display(Name = "Profile Picture URL (optional)")]
        public string? ProfilePictureUrl { get; set; }

        [Display(Name = "Location")]
        public string? Location { get; set; }
    }
}
