using System.Collections.Generic;

namespace MediCareConnect.Models
{
    public class PatientDashboardViewModel
    {
        public Patient Patient { get; set; } = null!;
        public List<Appointment> UpcomingAppointments { get; set; } = new List<Appointment>();
        public List<Doctor> RecommendedDoctors { get; set; } = new List<Doctor>();
    }
}
