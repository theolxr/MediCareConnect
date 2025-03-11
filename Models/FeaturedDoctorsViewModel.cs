using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediCareConnect.Models
{
    public class FeaturedDoctorsViewModel
    {
        public string? Search { get; set; }
        public string? SelectedSpecialty { get; set; }  // Added property
        public string? SelectedLocation { get; set; }   // Added property
        public List<Doctor> Doctors { get; set; } = new List<Doctor>();
        public IEnumerable<SelectListItem>? Specialties { get; set; }
        public IEnumerable<SelectListItem>? Locations { get; set; }
        public List<int> FavoritedDoctorIds { get; set; } = new List<int>();
    }
}
