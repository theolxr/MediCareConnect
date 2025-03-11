using Microsoft.AspNetCore.Mvc;
using MediCareConnect.Data;
using MediCareConnect.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MediCareConnect.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DoctorsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Doctors/FeaturedDoctors
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> FeaturedDoctors(string? search, string? selectedSpecialty, string? selectedLocation)
        {
            // Build the doctor query.
            var query = _context.Doctors.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.FullName.Contains(search));
            }

            if (!string.IsNullOrEmpty(selectedSpecialty))
            {
                query = query.Where(d => d.Specialty == selectedSpecialty);
            }

            if (!string.IsNullOrEmpty(selectedLocation))
            {
                query = query.Where(d => d.Location == selectedLocation);
            }

            var doctorsList = await query.OrderBy(d => d.FullName).ToListAsync();

            // Load dropdown values for specialties and locations.
            var specialties = await _context.Doctors
                .Where(d => !string.IsNullOrEmpty(d.Specialty))
                .Select(d => d.Specialty)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            var locations = await _context.Doctors
                .Where(d => !string.IsNullOrEmpty(d.Location))
                .Select(d => d.Location)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();

            var specialtyItems = specialties.Select(s => new SelectListItem { Value = s, Text = s }).ToList();
            var locationItems = locations.Select(l => new SelectListItem { Value = l, Text = l }).ToList();

            // Build the view model.
            var model = new DoctorSearchViewModel
            {
                Specialty = selectedSpecialty,
                Location = selectedLocation,
                Search = search,
                Doctors = doctorsList,
                Specialties = specialtyItems,
                Locations = locationItems,
                SelectedSpecialty = selectedSpecialty,
                SelectedLocation = selectedLocation,
                FavoritedDoctorIds = new List<int>()
            };

            // If the current user is a patient, load their favorited doctor IDs.
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
                if (patient != null)
                {
                    model.FavoritedDoctorIds = await _context.FavoriteDoctors
                        .Where(f => f.PatientId == patient.Id)
                        .Select(f => f.DoctorId)
                        .ToListAsync();
                }
            }

            return View(model);
        }

        // GET: /Doctors/Details/{id}
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            var reviews = await _context.DoctorReviews
                .Where(r => r.DoctorId == id)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            double averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            bool isFavorited = false;
            // If the current user is a patient, determine if they favorited this doctor.
            if (User.IsInRole("Patient"))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
                    if (patient != null)
                    {
                        isFavorited = await _context.FavoriteDoctors.AnyAsync(f => f.PatientId == patient.Id && f.DoctorId == id);
                    }
                }
            }

            var model = new DoctorDetailsViewModel
            {
                Doctor = doctor,
                AverageRating = averageRating,
                Reviews = reviews,
                Appointments = appointments,
                IsFavorited = isFavorited
            };

            return View("Details",model);
        }

        // GET: /Doctors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Doctors/Create
        [HttpPost]
        public async Task<IActionResult> Create(Doctor model)
        {
            if (ModelState.IsValid)
            {
                _context.Doctors.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
}
