using System.ComponentModel.DataAnnotations;

namespace Vehicle_Service_Management_System.Application.ViewModels.VehicleType
{
    // ─── LIST VIEW MODEL ───
    public class VehicleTypeListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Type Name")]
        public string TypeName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool IsDeleted { get; set; }

        // ✅ Correct status – Active when IsDeleted = false, Inactive when true
        public string Status => IsDeleted ? "Inactive" : "Active";

        // ✅ Correct badge – green for Active, red for Inactive
        public string StatusBadge => IsDeleted ? "bg-danger" : "bg-success";

        [Display(Name = "Total Vehicles")]
        public int VehicleCount { get; set; }

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }
    }

    // ─── FORM VIEW MODEL (Create/Edit) ───
    public class VehicleTypeFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Type name is required")]
        [StringLength(100, ErrorMessage = "Type name cannot exceed 100 characters")]
        [Display(Name = "Type Name")]
        public string TypeName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // ✅ Default is false (Active). If you want a checkbox to set inactive at creation, keep it as bool.
        [Display(Name = "Is Deleted (Inactive)")]
        public bool IsDeleted { get; set; } = false;   // changed from true to false
    }

    // ─── DETAILS VIEW MODEL ───
    public class VehicleTypeDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Type Name")]
        public string TypeName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool IsDeleted { get; set; }

        public string Status => IsDeleted ? "Inactive" : "Active";
        public string StatusBadge => IsDeleted ? "bg-danger" : "bg-success";

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Modified Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? ModifiedOn { get; set; }

        [Display(Name = "Total Vehicles")]
        public int TotalVehicles { get; set; }

        [Display(Name = "Active Vehicles")]
        public int ActiveVehicles { get; set; }

        public List<VehicleSummaryViewModel> Vehicles { get; set; } = new();
    }

    // ─── CREATE VIEW MODEL (optional, if you don't want to use the same form) ───
    public class VehicleTypeCreateViewModel
    {
        [Required(ErrorMessage = "Type name is required")]
        [StringLength(100, ErrorMessage = "Type name cannot exceed 100 characters")]
        [Display(Name = "Type Name")]
        public string TypeName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }

    // ─── UPDATE VIEW MODEL (if you want separate) ───
    public class VehicleTypeUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Type name is required")]
        [StringLength(100, ErrorMessage = "Type name cannot exceed 100 characters")]
        [Display(Name = "Type Name")]
        public string TypeName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Is Deleted (Inactive)")]
        public bool IsDeleted { get; set; }
    }

    // ─── SEARCH VIEW MODEL ───
    public class VehicleTypeSearchViewModel
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Include Deleted")]
        public bool? IncludeDeleted { get; set; }   // better name than "IsDeleted"

        [Display(Name = "Has Vehicles")]
        public bool? HasVehicles { get; set; }
    }

    // ─── STATISTICS VIEW MODEL ───
    public class VehicleTypeStatisticsViewModel
    {
        [Display(Name = "Total Types")]
        public int TotalTypes { get; set; }

        [Display(Name = "Active Types")]
        public int ActiveTypes { get; set; }

        [Display(Name = "Inactive Types")]
        public int InactiveTypes { get; set; }

        [Display(Name = "Total Vehicles")]
        public int TotalVehicles { get; set; }

        [Display(Name = "Type with Most Vehicles")]
        public string? TypeWithMostVehicles { get; set; }

        [Display(Name = "Average Vehicles per Type")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AverageVehiclesPerType { get; set; }

        public Dictionary<string, int> VehicleDistribution { get; set; } = new();
    }

    // ─── VEHICLE SUMMARY (used in Details) ───
    public class VehicleSummaryViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; } = string.Empty;

        [Display(Name = "Brand")]
        public string BrandName { get; set; } = string.Empty;

        [Display(Name = "Manufacturer Year")]
        public int? ManufacturerYear { get; set; }

        [Display(Name = "Color")]
        public string? Color { get; set; }

        public bool IsDeleted { get; set; }

        public string Status => IsDeleted ? "Inactive" : "Active";
        public string StatusBadge => IsDeleted ? "bg-danger" : "bg-success";
    }
}