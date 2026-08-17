using System.ComponentModel.DataAnnotations;

namespace Vehicle_Service_Management_System.Application.ViewModels.ServiceType
{
    // ─── LIST VIEW MODEL ───
    public class ServiceTypeListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Service Name")]
        public string ServiceName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Labour Charge")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal LabourCharge { get; set; }

        [Display(Name = "Estimated Hours")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public decimal EstimatedHours { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        // ✅ Correct computed status
        public string Status => (IsDeleted || !IsActive) ? "Inactive" : "Active";

        // ✅ Correct badge – green for Active, red for Inactive
        public string StatusBadge => (IsDeleted || !IsActive) ? "bg-danger" : "bg-success";

        [Display(Name = "Total Bookings")]
        public int TotalBookings { get; set; }

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }
    }

    // ─── FORM VIEW MODEL (Create/Edit) ───
    public class ServiceTypeFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Service name is required")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters")]
        [Display(Name = "Service Name")]
        public string ServiceName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Labour charge is required")]
        [Range(0.01, 100000, ErrorMessage = "Labour charge must be between 0.01 and 100,000")]
        [Display(Name = "Labour Charge")]
        [DataType(DataType.Currency)]
        public decimal LabourCharge { get; set; }

        [Required(ErrorMessage = "Estimated hours is required")]
        [Range(0.1, 240, ErrorMessage = "Estimated hours must be between 0.1 and 240")]
        [Display(Name = "Estimated Hours")]
        public decimal EstimatedHours { get; set; }

        // ✅ Default is false (Active). Change to true only if you want new services to be inactive.
        [Display(Name = "Is Deleted (Inactive)")]
        public bool IsDeleted { get; set; } = false;
    }

    // ─── DETAILS VIEW MODEL ───
    public class ServiceTypeDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Service Name")]
        public string ServiceName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Labour Charge")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal LabourCharge { get; set; }

        [Display(Name = "Estimated Hours")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public decimal EstimatedHours { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        public string Status => (IsDeleted || !IsActive) ? "Inactive" : "Active";
        public string StatusBadge => (IsDeleted || !IsActive) ? "bg-danger" : "bg-success";

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

        [Display(Name = "Total Revenue")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalRevenue { get; set; }
    }

    // ─── CREATE VIEW MODEL (optional, if you don't want to reuse the form) ───
    public class ServiceTypeCreateViewModel
    {
        [Required(ErrorMessage = "Service name is required")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters")]
        [Display(Name = "Service Name")]
        public string ServiceName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Labour charge is required")]
        [Range(0.01, 100000, ErrorMessage = "Labour charge must be between 0.01 and 100,000")]
        [Display(Name = "Labour Charge")]
        public decimal LabourCharge { get; set; }

        [Required(ErrorMessage = "Estimated hours is required")]
        [Range(0.1, 240, ErrorMessage = "Estimated hours must be between 0.1 and 240")]
        [Display(Name = "Estimated Hours")]
        public decimal EstimatedHours { get; set; }
    }

    // ─── UPDATE VIEW MODEL ───
    public class ServiceTypeUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Service name is required")]
        [StringLength(100, ErrorMessage = "Service name cannot exceed 100 characters")]
        [Display(Name = "Service Name")]
        public string ServiceName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Labour charge is required")]
        [Range(0.01, 100000, ErrorMessage = "Labour charge must be between 0.01 and 100,000")]
        [Display(Name = "Labour Charge")]
        public decimal LabourCharge { get; set; }

        [Required(ErrorMessage = "Estimated hours is required")]
        [Range(0.1, 240, ErrorMessage = "Estimated hours must be between 0.1 and 240")]
        [Display(Name = "Estimated Hours")]
        public decimal EstimatedHours { get; set; }

        [Display(Name = "Is Deleted (Inactive)")]
        public bool IsDeleted { get; set; }
    }

    // ─── SEARCH VIEW MODEL ───
    public class ServiceTypeSearchViewModel
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Min Labour Charge")]
        public decimal? MinLabourCharge { get; set; }

        [Display(Name = "Max Labour Charge")]
        public decimal? MaxLabourCharge { get; set; }

        [Display(Name = "Min Estimated Hours")]
        public decimal? MinEstimatedHours { get; set; }

        [Display(Name = "Max Estimated Hours")]
        public decimal? MaxEstimatedHours { get; set; }

        // ✅ Rename to clarify it's a filter, not the entity property
        [Display(Name = "Include Deleted")]
        public bool? IncludeDeleted { get; set; }
    }

    // ─── STATISTICS VIEW MODEL ───
    public class ServiceTypeStatisticsViewModel
    {
        [Display(Name = "Total Services")]
        public int TotalServices { get; set; }

        [Display(Name = "Active Services")]
        public int ActiveServices { get; set; }

        [Display(Name = "Inactive Services")]
        public int InactiveServices { get; set; }

        [Display(Name = "Average Labour Charge")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AverageLabourCharge { get; set; }

        [Display(Name = "Max Labour Charge")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal MaxLabourCharge { get; set; }

        [Display(Name = "Min Labour Charge")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal MinLabourCharge { get; set; }

        [Display(Name = "Total Bookings")]
        public int TotalBookings { get; set; }

        [Display(Name = "Total Revenue")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalRevenue { get; set; }

        public Dictionary<string, int> MostUsedServices { get; set; } = new();
    }
}