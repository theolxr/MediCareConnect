using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MediCareConnect.Data;
using MediCareConnect.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MediCareConnect.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ProfileController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Profile/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            // Check the user's role and try to load their profile.
            if (await _userManager.IsInRoleAsync(user, "Doctor"))
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.IdentityUserId == user.Id);
                // If no profile exists, redirect to create it.
                if (doctor == null)
                    return RedirectToAction("CreateDoctor");
                return View("DoctorProfile", doctor);
            }
            else if (await _userManager.IsInRoleAsync(user, "Patient"))
            {
                var patient = _context.Patients.FirstOrDefault(p => p.IdentityUserId == user.Id);
                if (patient == null)
                    return RedirectToAction("CreatePatient");
                return View("PatientProfile", patient);
            }
            else if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return View("AdminProfile");
            }
            else
            {
                // If the user has no recognized role, allow them to choose.
                return RedirectToAction("ChooseProfileType");
            }
        }

        // GET: /Profile/ChooseProfileType
        public IActionResult ChooseProfileType()
        {
            return View();
        }

       

        // GET: /Profile/CreatePatient
        public async Task<IActionResult> CreatePatient()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();
            if (!await _userManager.IsInRoleAsync(user, "Patient"))
                return Forbid();
            // Optionally, check if a patient profile already exists.
            var existing = _context.Patients.FirstOrDefault(p => p.IdentityUserId == user.Id);
            if (existing != null)
                return RedirectToAction("Index");
            return View();
        }

        // POST: /Profile/CreatePatient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePatient(CreatePatientViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();
            if (!await _userManager.IsInRoleAsync(user, "Patient"))
                return Forbid();

            if (ModelState.IsValid)
            {
                if (_context.Patients.Any(p => p.IdentityUserId == user.Id))
                {
                    return RedirectToAction("Index");
                }

                var patient = new Patient
                {
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    IdentityUserId = user.Id
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                // Ensure the user is in the Patient role.
                if (!await _userManager.IsInRoleAsync(user, "Patient"))
                {
                    await _userManager.AddToRoleAsync(user, "Patient");
                }

                return RedirectToAction("Index");
            }
            return View(model);
        }
        // GET: /Profile/CreateDoctor
        // Only accessible for users in the Doctor role.
        public async Task<IActionResult> CreateDoctor()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();
            if (!await _userManager.IsInRoleAsync(user, "Doctor"))
                return Forbid();
            if (_context.Doctors.Any(d => d.IdentityUserId == user.Id))
                return RedirectToAction("Index");
            return View();
        }

        // POST: /Profile/CreateDoctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(CreateDoctorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();
            if (!await _userManager.IsInRoleAsync(user, "Doctor"))
                return Forbid();

            if (ModelState.IsValid)
            {
                if (_context.Doctors.Any(d => d.IdentityUserId == user.Id))
                {
                    return RedirectToAction("Index");
                }

                var doctor = new Doctor
                {
                    FullName = model.FullName,
                    Specialty = model.Specialty,
                    ProfilePictureUrl = model.ProfilePictureUrl,
                    Location = model.Location, // set location
                    IdentityUserId = user.Id
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: /Profile/EditDoctor
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> EditDoctor()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await _userManager.IsInRoleAsync(user, "Doctor"))
                return Forbid();
            var doctor = _context.Doctors.FirstOrDefault(d => d.IdentityUserId == user.Id);
            if (doctor == null)
                return RedirectToAction("CreateDoctor");
            var model = new EditDoctorViewModel
            {
                FullName = doctor.FullName,
                Specialty = doctor.Specialty,
                ProfilePictureUrl = doctor.ProfilePictureUrl,
                Location = doctor.Location // pass location to view
            };
            return View(model);
        }

        // POST: /Profile/EditDoctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> EditDoctor(EditDoctorViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            if (!await _userManager.IsInRoleAsync(user, "Doctor"))
                return Forbid();
            if (ModelState.IsValid)
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.IdentityUserId == user.Id);
                if (doctor == null)
                    return RedirectToAction("CreateDoctor");

                doctor.FullName = model.FullName;
                doctor.Specialty = model.Specialty;
                doctor.ProfilePictureUrl = model.ProfilePictureUrl;
                doctor.Location = model.Location; // update location

                _context.Doctors.Update(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // ------------------------------------------
        // EDIT PATIENT PROFILE
        // ------------------------------------------

        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> EditPatient()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var patient = _context.Patients.FirstOrDefault(p => p.IdentityUserId == user.Id);
            if (patient == null)
            {
                return RedirectToAction("CreatePatient");
            }

            var model = new EditPatientViewModel
            {
                FullName = patient.FullName,
                PhoneNumber = patient.PhoneNumber,
                Email = patient.Email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> EditPatient(EditPatientViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var patient = _context.Patients.FirstOrDefault(p => p.IdentityUserId == user.Id);
            if (patient == null)
            {
                return RedirectToAction("CreatePatient");
            }

            patient.FullName = model.FullName;
            patient.PhoneNumber = model.PhoneNumber;
            patient.Email = model.Email;

            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        // GET: /Profile/DoctorDetails/5
        [AllowAnonymous]
        public IActionResult DoctorDetails(int id)
        {
            // Retrieve the doctor by id.
            var doctor = _context.Doctors.FirstOrDefault(d => d.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            // Retrieve all reviews for this doctor.
            var reviews = _context.DoctorReviews.Where(r => r.DoctorId == id);
            double averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

            var model = new DoctorPublicProfileViewModel
            {
                Doctor = doctor,
                AverageRating = averageRating,
                Reviews = reviews.OrderByDescending(r => r.CreatedDate).ToList()
            };

            // Use your existing view "DoctorPublicProfile" to display the data.
            return View("DoctorPublicProfile", model);
        }


        // GET: /Profile/PatientDetails/5
        [AllowAnonymous]
        public IActionResult PatientDetails(int id)
        {
            var patient = _context.Patients.FirstOrDefault(p => p.Id == id);
            if (patient == null)
                return NotFound();
            return View("PatientPublicProfile", patient);
        }


    }
}
