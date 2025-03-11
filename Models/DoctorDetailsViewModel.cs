using System.Collections.Generic;

namespace MediCareConnect.Models
{
    public class DoctorDetailsViewModel
    {
        public Doctor Doctor { get; set; } = null!;
        public double AverageRating { get; set; }
        public List<DoctorReview> Reviews { get; set; } = new List<DoctorReview>();
        // If you want to show upcoming or past appointments on this page
        public List<Appointment>? Appointments { get; set; }

        // New property indicating if the current patient has favorited this doctor
        public bool IsFavorited { get; set; }
    }
}
