using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediCareConnect.Models
{
    public class DoctorSearchViewModel
    {
        public string? Specialty { get; set; }
        public string? Location { get; set; }
        public string? Search { get; set; }
        public List<Doctor> Doctors { get; set; } = new List<Doctor>();
        public IEnumerable<SelectListItem>? Specialties { get; set; }
        public IEnumerable<SelectListItem>? Locations { get; set; }
        public List<int> FavoritedDoctorIds { get; set; } = new List<int>();

        // Add these new properties to store the currently selected values:
        public string? SelectedSpecialty { get; set; }
        public string? SelectedLocation { get; set; }
    }
}
