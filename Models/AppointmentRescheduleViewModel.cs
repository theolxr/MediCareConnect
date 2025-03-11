using System;
using System.ComponentModel.DataAnnotations;

namespace MediCareConnect.Models
{
    public class AppointmentRescheduleViewModel
    {
        [Required]
        public int AppointmentId { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "New Appointment Date and Time")]
        public DateTime NewAppointmentDate { get; set; }
    }
}
