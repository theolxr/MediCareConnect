using System;
using System.Collections.Generic;

namespace MediCareConnect.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalDoctors { get; set; }
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int FinishedAppointments { get; set; }

        // A list of recent appointments for quick review
        public List<Appointment> RecentAppointments { get; set; } = new List<Appointment>();
    }
}
