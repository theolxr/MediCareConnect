using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MediCareConnect.Data;
using MediCareConnect.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MediCareConnect.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Appointments/Index
        // GET: /Appointments/Index
        public async Task<IActionResult> Index()
        {
            // For Patients: show appointments that belong to them.
            // For Doctors: show appointments where they are assigned.
            // For Admin: show all appointments.
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            if (User.IsInRole("Patient"))
            {
                var patientProfile = await _context.Patients.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
                if (patientProfile == null)
                    return RedirectToAction("CreatePatient", "Profile");
                var appointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .Where(a => a.PatientId == patientProfile.Id)
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync();
                return View(appointments);
            }
            else if (User.IsInRole("Doctor"))
            {
                var doctorProfile = await _context.Doctors.FirstOrDefaultAsync(d => d.IdentityUserId == user.Id);
                if (doctorProfile == null)
                    return RedirectToAction("CreateDoctor", "Profile");
                var appointments = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Where(a => a.DoctorId == doctorProfile.Id)
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync();
                return View(appointments);
            }
            else if (User.IsInRole("Admin"))
            {
                var appointments = await _context.Appointments
                    .Include(a => a.Doctor)
                    .Include(a => a.Patient)
                    .OrderBy(a => a.AppointmentDate)
                    .ToListAsync();
                return View(appointments);
            }
            else
            {
                // Default: show nothing or a message.
                return View(Enumerable.Empty<Appointment>());
            }
        }



        // GET: /Appointments/Create
        public IActionResult Create()
        {
            var model = new AppointmentViewModel
            {
                AppointmentDate = DateTime.Now.AddDays(1),
                Doctors = _context.Doctors
                    .OrderBy(d => d.FullName)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.FullName
                    }).ToList()
            };
            return View(model);
        }

        // POST: /Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Doctors = _context.Doctors
                    .OrderBy(d => d.FullName)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.FullName
                    }).ToList();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var patientProfile = await _context.Patients.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (patientProfile == null)
                return RedirectToAction("CreatePatient", "Profile");

            var appointment = new Appointment
            {
                AppointmentDate = model.AppointmentDate,
                Notes = model.Notes,
                DoctorId = model.DoctorId,
                PatientId = patientProfile.Id,
                Status = AppointmentStatus.Pending
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // --- Patient Actions ---

        // POST: /Appointments/Cancel
        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var patientProfile = await _context.Patients.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (patientProfile == null || appointment.PatientId != patientProfile.Id)
                return Forbid();

            appointment.Status = AppointmentStatus.Cancelled;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: /Appointments/Reschedule
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Reschedule(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var patientProfile = await _context.Patients.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (patientProfile == null || appointment.PatientId != patientProfile.Id)
                return Forbid();

            var model = new AppointmentRescheduleViewModel
            {
                AppointmentId = appointment.Id,
                NewAppointmentDate = appointment.AppointmentDate
            };
            return View(model);
        }

        // POST: /Appointments/Reschedule
        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(AppointmentRescheduleViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var appointment = await _context.Appointments.FindAsync(model.AppointmentId);
            if (appointment == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var patientProfile = await _context.Patients.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (patientProfile == null || appointment.PatientId != patientProfile.Id)
                return Forbid();

            appointment.AppointmentDate = model.NewAppointmentDate;
            appointment.Status = AppointmentStatus.Rescheduled;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Patient")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinishAndReview(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var patientProfile = await _context.Patients.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (patientProfile == null || appointment.PatientId != patientProfile.Id)
                return Forbid();

            appointment.Status = AppointmentStatus.Finished;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            // Return a JSON result with the doctor's ID and appointment ID
            return Json(new { success = true, doctorId = appointment.DoctorId, appointmentId = appointment.Id });
        }

        // --- Doctor Actions ---

        // POST: /Appointments/Confirm
        [Authorize(Roles = "Doctor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var doctorProfile = await _context.Doctors.FirstOrDefaultAsync(d => d.IdentityUserId == user.Id);
            if (doctorProfile == null || appointment.DoctorId != doctorProfile.Id)
                return Forbid();

            appointment.Status = AppointmentStatus.Confirmed;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: /Appointments/Reject
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Reject(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var doctorProfile = await _context.Doctors.FirstOrDefaultAsync(d => d.IdentityUserId == user.Id);
            if (doctorProfile == null || appointment.DoctorId != doctorProfile.Id)
                return Forbid();

            var model = new AppointmentRejectViewModel
            {
                AppointmentId = appointment.Id
            };
            return View(model);
        }

        // POST: /Appointments/Reject
        [Authorize(Roles = "Doctor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(AppointmentRejectViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var appointment = await _context.Appointments.FindAsync(model.AppointmentId);
            if (appointment == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var doctorProfile = await _context.Doctors.FirstOrDefaultAsync(d => d.IdentityUserId == user.Id);
            if (doctorProfile == null || appointment.DoctorId != doctorProfile.Id)
                return Forbid();

            appointment.Status = AppointmentStatus.Rejected;
            appointment.RejectionReason = model.RejectionReason;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // -------------------
        // ADMIN EDIT/DELETE APPOINTMENTS
        // -------------------

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminEdit(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            var model = new AdminEditAppointmentViewModel
            {
                AppointmentId = appointment.Id,
                AppointmentDate = appointment.AppointmentDate,
                Notes = appointment.Notes,
                Status = appointment.Status,
                RejectionReason = appointment.RejectionReason
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminEdit(AdminEditAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var appointment = await _context.Appointments.FindAsync(model.AppointmentId);
            if (appointment == null)
                return NotFound();

            appointment.AppointmentDate = model.AppointmentDate;
            appointment.Notes = model.Notes;
            appointment.Status = model.Status;
            appointment.RejectionReason = model.RejectionReason;

            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDelete(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
                return NotFound();
            return View(appointment);
        }

        [HttpPost, ActionName("AdminDelete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
