using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediCareConnect.Models
{
    public class UserRoleViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please select a role.")]
        public string SelectedRole { get; set; }

        // The available roles for assignment
        [ValidateNever]
        public IEnumerable<SelectListItem> Roles { get; set; }
    }
}
