using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MediCareConnect.Models
{
    public class Patient
    {
        [Key]  // Optional if property is named "Id"
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = null!;

        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        // This is set programmatically to link the profile with the Identity user.
        [Required]
        public string IdentityUserId { get; set; } = null!;

        public ICollection<Appointment>? Appointments { get; set; }
    }
}
