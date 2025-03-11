using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MediCareConnect.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediCareConnect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: /Admin/AssignRole
        public IActionResult AssignRole()
        {
            var model = new UserRoleViewModel
            {
                Roles = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Doctor", Value = "Doctor" },
                    new SelectListItem { Text = "Patient", Value = "Patient" }
                }
            };
            return View(model);
        }

        // POST: /Admin/AssignRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(UserRoleViewModel model)
        {
            System.Diagnostics.Debug.WriteLine($"AssignRole POST triggered. Email: '{model.Email}', SelectedRole: '{model.SelectedRole}'");

            if (!ModelState.IsValid)
            {
                ReloadRoles(model);
                return View(model);
            }

            // Trim the email and lookup user
            var email = model.Email.Trim();
            System.Diagnostics.Debug.WriteLine($"Looking up user with email: '{email}'");
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                System.Diagnostics.Debug.WriteLine($"User with email '{email}' not found.");
                ModelState.AddModelError("", $"User with email '{email}' not found.");
                ReloadRoles(model);
                return View(model);
            }

            // Remove all roles from the user (if any)
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removalResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removalResult.Succeeded)
                {
                    foreach (var error in removalResult.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error removing role: {error.Description}");
                        ModelState.AddModelError("", error.Description);
                    }
                    ReloadRoles(model);
                    return View(model);
                }
                System.Diagnostics.Debug.WriteLine($"Removed roles: {string.Join(", ", currentRoles)} from '{email}'.");
            }

            // Assign the selected role
            var addResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);
            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                {
                    System.Diagnostics.Debug.WriteLine($"Error adding role: {error.Description}");
                    ModelState.AddModelError("", error.Description);
                }
                ReloadRoles(model);
                return View(model);
            }

            System.Diagnostics.Debug.WriteLine($"Successfully assigned role '{model.SelectedRole}' to '{email}'.");
            TempData["Message"] = $"User '{email}' has been assigned the role '{model.SelectedRole}'.";
            return RedirectToAction("AssignRole");
        }

        private void ReloadRoles(UserRoleViewModel model)
        {
            model.Roles = new List<SelectListItem>
            {
                new SelectListItem { Text = "Doctor", Value = "Doctor" },
                new SelectListItem { Text = "Patient", Value = "Patient" }
            };
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
