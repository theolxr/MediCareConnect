using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MediCareConnect.Models
{
    public class AppointmentViewModel
    {
        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Appointment Date and Time")]
        public DateTime AppointmentDate { get; set; }

        [Display(Name = "Notes (optional)")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Please select a doctor.")]
        [Display(Name = "Doctor")]
        public int DoctorId { get; set; }

        // This property is only used to populate the dropdown list.
        [ValidateNever]
        public IEnumerable<SelectListItem> Doctors { get; set; } = new List<SelectListItem>();
    }
}
