using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MediCareConnect.Data;
using MediCareConnect.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediCareConnect.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DoctorDashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /DoctorDashboard/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            // Retrieve the doctor profile using the IdentityUserId.
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.IdentityUserId == user.Id);
            if (doctor == null)
            {
                // If no doctor profile exists, redirect to create one.
                return RedirectToAction("CreateDoctor", "Profile");
            }

            // Retrieve upcoming appointments for this doctor (appointments with a future date and not Finished).
            var upcomingAppointments = await _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id &&
                            a.AppointmentDate >= DateTime.Now &&
                            a.Status != AppointmentStatus.Finished)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            var model = new DoctorDashboardViewModel
            {
                Doctor = doctor,
                UpcomingAppointments = upcomingAppointments
            };

            return View(model);
        }

        public async Task<IActionResult> GetAppointments(DateTime start, DateTime end)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.IdentityUserId == user.Id);
            if (doctor == null) return NotFound("Doctor profile not found.");

            // Fetch all appointments within the requested date range
            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate >= start && a.AppointmentDate < end)
                .ToListAsync();

            // Group appointments by their date (year-month-day)
            var groupedByDay = appointments
                .GroupBy(a => a.AppointmentDate.Date)
                .Select(g => new {
                    Date = g.Key, // The day
                    Statuses = g.Select(a => a.Status).Distinct() // All statuses that occur on that day
                })
                .ToList();

            // For each day, decide which color to use based on the statuses
            var backgroundEvents = new List<object>();

            foreach (var dayGroup in groupedByDay)
            {
                // Example logic: If any appointment is Pending, color is Yellow;
                // else if any Confirmed, color is Red; else if any Finished, color is Green;
                // otherwise no color.
                var statuses = dayGroup.Statuses;

                // Decide color based on priority
                string color = null;
                if (statuses.Contains(AppointmentStatus.Pending))
                    color = "yellow";
                else if (statuses.Contains(AppointmentStatus.Confirmed))
                    color = "red";
                else if (statuses.Contains(AppointmentStatus.Finished))
                    color = "green";
                // Optionally handle other statuses (Cancelled, Rejected, etc.)

                // If color is determined, create a background event
                if (!string.IsNullOrEmpty(color))
                {
                    backgroundEvents.Add(new
                    {
                        start = dayGroup.Date.ToString("yyyy-MM-dd"),
                        end = dayGroup.Date.AddDays(1).ToString("yyyy-MM-dd"), // FullCalendar uses exclusive end
                        display = "background",
                        backgroundColor = color,
                        borderColor = color,
                        allDay = true  // ensure it spans the entire day cell
                    });
                }
            }

            return Json(backgroundEvents);
        }

        // 1) Returns background events for the entire range, coloring the cell based on statuses that day.
        public async Task<IActionResult> GetBackgroundEvents(DateTime start, DateTime end)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.IdentityUserId == user.Id);
            if (doctor == null) return NotFound("Doctor profile not found.");

            // Fetch appointments in [start, end)
            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate >= start && a.AppointmentDate < end)
                .ToListAsync();

            // Group by day
            var groupedByDay = appointments
                .GroupBy(a => a.AppointmentDate.Date)
                .Select(g => new {
                    Date = g.Key,
                    Statuses = g.Select(a => a.Status).Distinct()
                })
                .ToList();

            var backgroundEvents = new List<object>();

            foreach (var dayGroup in groupedByDay)
            {
                // Priority logic: If any 'Pending', color = yellow; else if any 'Confirmed', color = red; else if any 'Finished', color = green.
                // Adjust logic if you have more statuses or different priorities.
                var statuses = dayGroup.Statuses;
                string color = null;

                if (statuses.Contains(AppointmentStatus.Pending))
                    color = "yellow";
                else if (statuses.Contains(AppointmentStatus.Confirmed))
                    color = "red";
                else if (statuses.Contains(AppointmentStatus.Finished))
                    color = "green";
                // etc. for other statuses if needed

                if (!string.IsNullOrEmpty(color))
                {
                    // Create a background event that spans the entire day
                    backgroundEvents.Add(new
                    {
                        start = dayGroup.Date.ToString("yyyy-MM-dd"),
                        end = dayGroup.Date.AddDays(1).ToString("yyyy-MM-dd"), // exclusive end
                        display = "background",
                        backgroundColor = color,
                        borderColor = color,
                        allDay = true
                    });
                }
            }

            return Json(backgroundEvents);
        }

        // 2) Returns day-based appointments for the clicked date
        public async Task<IActionResult> GetAppointmentsByDay(DateTime date)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.IdentityUserId == user.Id);
            if (doctor == null) return NotFound("Doctor profile not found.");

            // Return all appointments exactly on this day
            var dayAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate.Date == date.Date)
                .Select(a => new {
                    appointmentDate = a.AppointmentDate,
                    notes = a.Notes,
                    patientFullName = a.Patient.FullName,
                    status = a.Status.ToString()
                })
                .ToListAsync();

            return Json(dayAppointments);
        }

    }
}
