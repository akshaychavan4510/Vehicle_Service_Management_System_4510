// Vehicle_Service_Management_System.Application/ViewModels/Mechanic/MechanicViewModels.cs
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Vehicle_Service_Management_System.Application.ViewModels.Mechanic
{
    public class MechanicListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Specialization")]
        public string? Specialization { get; set; }

        [Display(Name = "Experience")]
        public int ExperienceYears { get; set; }

        [Display(Name = "Salary")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Salary { get; set; }

        [Display(Name = "Status")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Availability")]
        public string AvailabilityStatus => IsAvailable ? "Available" : "Busy";

        [Display(Name = "Status Badge")]
        public string StatusBadge => IsAvailable ? "bg-success" : "bg-danger";

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }
    }

    public class MechanicDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Specialization")]
        public string? Specialization { get; set; }

        [Display(Name = "Experience Years")]
        public int ExperienceYears { get; set; }

        [Display(Name = "Salary")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Salary { get; set; }

        [Display(Name = "Status")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Availability")]
        public string AvailabilityStatus => IsAvailable ? "Available" : "Busy";

        [Display(Name = "Status Badge")]
        public string StatusBadge => IsAvailable ? "bg-success" : "bg-danger";

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Modified Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? ModifiedOn { get; set; }

        [Display(Name = "Total Bookings")]
        public int TotalBookings { get; set; }

        [Display(Name = "Active Bookings")]
        public int ActiveBookings { get; set; }
    }

    public class MechanicFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "Specialization cannot exceed 50 characters")]
        [Display(Name = "Specialization")]
        public string? Specialization { get; set; }

        [Required(ErrorMessage = "Experience years is required")]
        [Range(0, 50, ErrorMessage = "Experience years must be between 0 and 50")]
        [Display(Name = "Experience Years")]
        public int ExperienceYears { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(0, 999999.99, ErrorMessage = "Salary must be between 0 and 999,999.99")]
        [Display(Name = "Salary")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Salary { get; set; }

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;
    }

    public class MechanicCreateViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "Specialization cannot exceed 50 characters")]
        [Display(Name = "Specialization")]
        public string? Specialization { get; set; }

        [Required(ErrorMessage = "Experience years is required")]
        [Range(0, 50, ErrorMessage = "Experience years must be between 0 and 50")]
        [Display(Name = "Experience Years")]
        public int ExperienceYears { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(0, 999999.99, ErrorMessage = "Salary must be between 0 and 999,999.99")]
        [Display(Name = "Salary")]
        public decimal Salary { get; set; }

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; } = true;
    }

    public class MechanicUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(50, ErrorMessage = "Specialization cannot exceed 50 characters")]
        [Display(Name = "Specialization")]
        public string? Specialization { get; set; }

        [Required(ErrorMessage = "Experience years is required")]
        [Range(0, 50, ErrorMessage = "Experience years must be between 0 and 50")]
        [Display(Name = "Experience Years")]
        public int ExperienceYears { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(0, 999999.99, ErrorMessage = "Salary must be between 0 and 999,999.99")]
        [Display(Name = "Salary")]
        public decimal Salary { get; set; }

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; }
    }

    public class MechanicAvailabilityViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Specialization")]
        public string? Specialization { get; set; }

        [Display(Name = "Is Available")]
        public bool IsAvailable { get; set; }

        [Display(Name = "Current Bookings")]
        public int CurrentBookings { get; set; }

        [Display(Name = "Max Capacity")]
        public int MaxCapacity { get; set; } = 3;

        [Display(Name = "Availability")]
        public string AvailabilityStatus => IsAvailable ? "Available" : "Busy";

        [Display(Name = "Status Badge")]
        public string StatusBadge => IsAvailable ? "bg-success" : "bg-danger";
    }

    public class MechanicStatisticsViewModel
    {
        [Display(Name = "Total Mechanics")]
        public int Total { get; set; }

        [Display(Name = "Available")]
        public int Available { get; set; }

        [Display(Name = "Busy")]
        public int Busy { get; set; }

        [Display(Name = "Average Experience")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AverageExperience { get; set; }

        [Display(Name = "Average Salary")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AverageSalary { get; set; }

        [Display(Name = "Total Salary Cost")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalSalaryCost { get; set; }

        public Dictionary<string, int> SpecializationDistribution { get; set; } = new();
    }

    public class MechanicSearchViewModel
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Specialization")]
        public string? Specialization { get; set; }

        [Display(Name = "Availability")]
        public bool? IsAvailable { get; set; }

        [Display(Name = "Min Experience")]
        public int? MinExperience { get; set; }

        [Display(Name = "Max Experience")]
        public int? MaxExperience { get; set; }

        [Display(Name = "Min Salary")]
        public decimal? MinSalary { get; set; }

        [Display(Name = "Max Salary")]
        public decimal? MaxSalary { get; set; }
    }
}