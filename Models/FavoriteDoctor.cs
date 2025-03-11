using System.ComponentModel.DataAnnotations;

namespace MediCareConnect.Models
{
    public class FavoriteDoctor
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        [Required]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
    }
}
