using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MediCareConnect.Data;
using MediCareConnect.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace MediCareConnect.Controllers
{
    [Authorize(Roles = "Patient")]
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Reviews/Create?doctorId=5
        public IActionResult Create(int doctorId)
        {
            var model = new DoctorReviewViewModel { DoctorId = doctorId };
            return View(model);
        }

        // POST: /Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorReviewViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var patientProfile = await _context.Patients.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (patientProfile == null)
                return RedirectToAction("CreatePatient", "Profile");

            var review = new DoctorReview
            {
                DoctorId = model.DoctorId,
                PatientId = patientProfile.Id,
                Rating = model.Rating,
                ReviewText = model.ReviewText
            };

            _context.DoctorReviews.Add(review);
            await _context.SaveChangesAsync();

            // Redirect to the doctor's public profile after review submission.
            return RedirectToAction("DoctorDetails", "Profile", new { id = model.DoctorId });
        }
    }
}
