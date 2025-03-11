using System;
using System.ComponentModel.DataAnnotations;

namespace MediCareConnect.Models
{
    public enum AppointmentStatus
    {
        Pending,
        Confirmed,
        Finished,
        Rejected,
        Cancelled,
        Rescheduled
    }

    public class Appointment
    {
        public int Id { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public string? Notes { get; set; }

        [Required]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public string? RejectionReason { get; set; }
    }
}
