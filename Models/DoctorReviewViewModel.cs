using System.ComponentModel.DataAnnotations;

namespace MediCareConnect.Models
{
    public class DoctorReviewViewModel
    {
        public int DoctorId { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Please select a rating between 1 and 5.")]
        public int Rating { get; set; }

        [Display(Name = "Your Review (optional)")]
        public string? ReviewText { get; set; }
    }
}
