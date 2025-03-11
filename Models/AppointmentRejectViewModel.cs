using System.ComponentModel.DataAnnotations;

namespace MediCareConnect.Models
{
    public class AppointmentRejectViewModel
    {
        [Required]
        public int AppointmentId { get; set; }

        [Display(Name = "Reason for Rejection (optional)")]
        public string? RejectionReason { get; set; }
    }
}
