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
    public class DashboardController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: /Dashboard/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            string displayName = user.Email;
            string roleLabel = "";

            // Use the ClaimsPrincipal's IsInRole method
            if (User.IsInRole("Doctor"))
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.IdentityUserId == user.Id);
                roleLabel = "Doctor";
                if (doctor != null && !string.IsNullOrWhiteSpace(doctor.FullName))
                    displayName = doctor.FullName;
            }
            else if (User.IsInRole("Patient"))
            {
                var patient = _context.Patients.FirstOrDefault(p => p.IdentityUserId == user.Id);
                roleLabel = "Patient";
                if (patient != null && !string.IsNullOrWhiteSpace(patient.FullName))
                    displayName = patient.FullName;
            }
            else if (User.IsInRole("Admin"))
            {
                roleLabel = "Admin";
                displayName = "Admin";
            }
            else
            {
                roleLabel = "User";
            }

            ViewBag.DisplayName = displayName;
            ViewBag.RoleLabel = roleLabel;
            return View();
        }
    }
}
