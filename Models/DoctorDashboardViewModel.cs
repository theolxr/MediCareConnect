using System.Collections.Generic;

namespace MediCareConnect.Models
{
    public class DoctorDashboardViewModel
    {
        public Doctor Doctor { get; set; } = null!;
        public List<Appointment> UpcomingAppointments { get; set; } = new List<Appointment>();
    }
}
