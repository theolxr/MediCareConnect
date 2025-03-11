using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MediCareConnect.Models
{
    public class Doctor
    {
        [Key]  // Optional if property is named "Id"
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = null!;

        public string? Specialty { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Location { get; set; }


        // Set programmatically from the logged-in Identity user.
        [Required]
        public string IdentityUserId { get; set; } = null!;

        public ICollection<Appointment>? Appointments { get; set; }
    }
}
