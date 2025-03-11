using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediCareConnect.Data;
using MediCareConnect.Models;

namespace MediCareConnect.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // New property to hold additional profile information
        public ProfileSummaryViewModel? UserProfile { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // In case user is null, you might redirect or challenge
                return;
            }

            Email = user.Email ?? string.Empty;
            PhoneNumber = user.PhoneNumber ?? string.Empty;

            // Load additional profile data based on the user's role
            if (await _userManager.IsInRoleAsync(user, "Doctor"))
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.IdentityUserId == user.Id);
                if (doctor != null)
                {
                    UserProfile = new ProfileSummaryViewModel
                    {
                        FullName = doctor.FullName,
                        Role = "Doctor"
                    };
                }
            }
            else if (await _userManager.IsInRoleAsync(user, "Patient"))
            {
                var patient = _context.Patients.FirstOrDefault(p => p.IdentityUserId == user.Id);
                if (patient != null)
                {
                    UserProfile = new ProfileSummaryViewModel
                    {
                        FullName = patient.FullName,
                        Role = "Patient"
                    };
                }
            }
            else if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                // For Admin, we simply use "Admin"
                UserProfile = new ProfileSummaryViewModel
                {
                    FullName = "Admin",
                    Role = "Admin"
                };
            }
        }
    }
}
