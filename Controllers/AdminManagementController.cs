using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCareConnect.Data;
using MediCareConnect.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MediCareConnect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // List all Doctors with search
        public async Task<IActionResult> Doctors(string? search)
        {
            var query = _context.Doctors.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.FullName.Contains(search));
            }
            var doctors = await query.OrderBy(d => d.FullName).ToListAsync();
            ViewBag.Search = search;
            return View(doctors);
        }

        // GET: Edit Doctor
        public async Task<IActionResult> EditDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound();

            var model = new EditDoctorViewModel
            {
                FullName = doctor.FullName,
                Specialty = doctor.Specialty,
                ProfilePictureUrl = doctor.ProfilePictureUrl,
                Location = doctor.Location
            };

            return View(model);
        }

        // POST: Edit Doctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDoctor(int id, EditDoctorViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound();

            doctor.FullName = model.FullName;
            doctor.Specialty = model.Specialty;
            doctor.ProfilePictureUrl = model.ProfilePictureUrl;
            doctor.Location = model.Location;

            _context.Update(doctor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Doctors));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound();

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Doctors));
        }

        // List all Patients with search
        public async Task<IActionResult> Patients(string? search)
        {
            var query = _context.Patients.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.FullName.Contains(search));
            }
            var patients = await query.OrderBy(p => p.FullName).ToListAsync();
            ViewBag.Search = search;
            return View(patients);
        }

        // GET: Edit Patient
        public async Task<IActionResult> EditPatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound();

            var model = new EditPatientViewModel
            {
                FullName = patient.FullName,
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email
            };

            return View(model);
        }

        // POST: Edit Patient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(int id, EditPatientViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound();

            patient.FullName = model.FullName;
            patient.PhoneNumber = model.PhoneNumber;
            patient.Email = model.Email;

            _context.Update(patient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Patients));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound();

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Patients));
        }

        // List all Appointments with search
        public async Task<IActionResult> Appointments(string? search)
        {
            var query = _context.Appointments
                        .Include(a => a.Doctor)
                        .Include(a => a.Patient)
                        .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => (a.Doctor != null && a.Doctor.FullName.Contains(search))
                                       || (a.Patient != null && a.Patient.FullName.Contains(search)));
            }

            var appointments = await query.OrderByDescending(a => a.AppointmentDate).ToListAsync();
            ViewBag.Search = search;
            return View("~/Views/AdminManagement/Appointments.cshtml", appointments);
        }

        // GET: Edit Appointment
        public async Task<IActionResult> EditAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            var model = new EditAppointmentViewModel
            {
                AppointmentId = appointment.Id,
                AppointmentDate = appointment.AppointmentDate,
                Notes = appointment.Notes,
                Status = appointment.Status,
                RejectionReason = appointment.RejectionReason
            };

            return View("~/Views/AdminManagement/EditAppointment.cshtml", model);
        }

        // POST: Edit Appointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAppointment(EditAppointmentViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/AdminManagement/EditAppointment.cshtml", model);

            var appointment = await _context.Appointments.FindAsync(model.AppointmentId);
            if (appointment == null)
                return NotFound();

            appointment.AppointmentDate = model.AppointmentDate;
            appointment.Notes = model.Notes;
            appointment.Status = model.Status;
            appointment.RejectionReason = model.RejectionReason;

            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Appointments));
        }

        // GET: Delete Appointment Confirmation
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
                return NotFound();

            return View("~/Views/AdminManagement/DeleteAppointment.cshtml", appointment);
        }

        // POST: Delete Appointment
        [HttpPost, ActionName("DeleteAppointment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Appointments));
        }
    }
}
