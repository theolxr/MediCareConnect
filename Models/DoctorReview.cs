using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MediCareConnect.Models
{
    public class DoctorReview
    {
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient? Patient { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        [DataType(DataType.MultilineText)]
        public string? ReviewText { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
