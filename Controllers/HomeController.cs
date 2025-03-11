using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediCareConnect.Data;
using MediCareConnect.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MediCareConnect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /
        // or GET: /Home/Index
        public async Task<IActionResult> Index(string? search, string? selectedSpecialty, string? selectedLocation)
        {
            if (User.IsInRole("Patient"))
            {
                return RedirectToAction("FeaturedDoctors", "Doctors");
            }
            // 1. Build the dropdown data for specialties and locations
            var allSpecialties = await _context.Doctors
                .Where(d => d.Specialty != null && d.Specialty != "")
                .Select(d => d.Specialty!)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            var allLocations = await _context.Doctors
                .Where(d => d.Location != null && d.Location != "")
                .Select(d => d.Location!)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();

            // Convert them to SelectListItem
            var specialtyItems = allSpecialties
                .Select(s => new SelectListItem { Value = s, Text = s })
                .ToList();

            var locationItems = allLocations
                .Select(l => new SelectListItem { Value = l, Text = l })
                .ToList();

            // 2. Start with all doctors
            var query = _context.Doctors.AsQueryable();

            // 3. Filter by search (doctor name)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.FullName.Contains(search));
            }

            // 4. Filter by selected specialty
            if (!string.IsNullOrEmpty(selectedSpecialty))
            {
                query = query.Where(d => d.Specialty == selectedSpecialty);
            }

            // 5. Filter by selected location
            if (!string.IsNullOrEmpty(selectedLocation))
            {
                query = query.Where(d => d.Location == selectedLocation);
            }

            // 6. Execute query
            var filteredDoctors = await query
                .OrderBy(d => d.FullName)
                .ToListAsync();

            // 7. Build the view model
            var model = new FeaturedDoctorsViewModel
            {
                Search = search,
                SelectedSpecialty = selectedSpecialty,
                SelectedLocation = selectedLocation,
                Specialties = specialtyItems,
                Locations = locationItems,
                Doctors = filteredDoctors
            };

            // 8. Return the Index view with the model
            return View(model);
        }
    }
}
