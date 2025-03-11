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
    [Authorize(Roles = "Patient")]
    public class PatientDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientDashboardController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /PatientDashboard/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            // Look up the patient profile using the IdentityUserId.
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (patient == null)
            {
                // If the patient hasn't created a profile, redirect to profile creation.
                return RedirectToAction("CreatePatient", "Profile");
            }

            // Retrieve upcoming appointments for this patient (appointments with date >= now)
            var upcomingAppointments = await _context.Appointments
             .Include(a => a.Doctor)
             .Where(a => a.PatientId == patient.Id
                         && a.AppointmentDate >= DateTime.Now
                         && a.Status != AppointmentStatus.Finished
                         && a.Status == AppointmentStatus.Confirmed)
             .OrderBy(a => a.AppointmentDate)
             .ToListAsync();


            // Retrieve recommended doctors (example: top 3 by name)
            // In PatientDashboardController:
            var recommendedDoctors = await _context.FavoriteDoctors
                .Where(f => f.PatientId == patient.Id)
                .Select(f => f.Doctor)
                .ToListAsync();


            var model = new PatientDashboardViewModel
            {
                Patient = patient,
                UpcomingAppointments = upcomingAppointments,
                RecommendedDoctors = recommendedDoctors
            };

            return View(model);
        }
    }
}
