using System;
using System.ComponentModel.DataAnnotations;

namespace MediCareConnect.Models
{
    public class EditAppointmentViewModel
    {
        public int AppointmentId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Appointment Date and Time")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Status")]
        public AppointmentStatus Status { get; set; }

        [Display(Name = "Rejection Reason (if applicable)")]
        public string? RejectionReason { get; set; }
    }
}
