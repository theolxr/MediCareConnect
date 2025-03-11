using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCareConnect.Data;
using MediCareConnect.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace MediCareConnect.Controllers
{
    [Authorize(Roles = "Patient")]
    public class FavoritesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public FavoritesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Favorites/GetFavoriteDoctorIds
        [HttpGet]
        public async Task<IActionResult> GetFavoriteDoctorIds()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (patient == null)
                return NotFound("Patient profile not found.");

            var favoriteIds = await _context.FavoriteDoctors
                .Where(f => f.PatientId == patient.Id)
                .Select(f => f.DoctorId)
                .ToListAsync();

            return Json(favoriteIds);
        }

        // POST: /Favorites/ToggleFavorite
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int doctorId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.IdentityUserId == user.Id);
            if (patient == null)
                return NotFound("Patient profile not found.");

            var favorite = await _context.FavoriteDoctors
                .FirstOrDefaultAsync(f => f.PatientId == patient.Id && f.DoctorId == doctorId);

            bool isFavorite;
            if (favorite != null)
            {
                _context.FavoriteDoctors.Remove(favorite);
                isFavorite = false;
            }
            else
            {
                favorite = new FavoriteDoctor { PatientId = patient.Id, DoctorId = doctorId };
                _context.FavoriteDoctors.Add(favorite);
                isFavorite = true;
            }
            await _context.SaveChangesAsync();
            return Json(new { success = true, isFavorite });
        }
    }
}
