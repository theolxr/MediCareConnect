using System;
using System.ComponentModel.DataAnnotations;
using MediCareConnect.Models; // For AppointmentStatus

namespace MediCareConnect.Models
{
    public class AdminEditAppointmentViewModel
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

        [Display(Name = "Rejection Reason (if rejected)")]
        public string? RejectionReason { get; set; }
    }
}
