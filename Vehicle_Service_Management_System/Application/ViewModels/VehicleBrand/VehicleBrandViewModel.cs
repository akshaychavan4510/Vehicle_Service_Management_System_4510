using System.ComponentModel.DataAnnotations;

namespace Vehicle_Service_Management_System.Application.ViewModels.VehicleBrand
{
    // ---------- LIST VIEW MODEL ----------
    public class VehicleBrandListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Brand Name")]
        public string BrandName { get; set; } = string.Empty;

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        [Display(Name = "Status")]
        public string Status => (IsDeleted || !IsActive) ? "Inactive" : "Active";

        [Display(Name = "Status Badge")]
        public string StatusBadge => (IsDeleted || !IsActive) ? "bg-danger" : "bg-success";

        [Display(Name = "Total Vehicles")]
        public int VehicleCount { get; set; }

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }
    }

    // ---------- DETAILS VIEW MODEL ----------
    public class VehicleBrandDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Brand Name")]
        public string BrandName { get; set; } = string.Empty;

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        [Display(Name = "Status")]
        public string Status => (IsDeleted || !IsActive) ? "Inactive" : "Active";

        [Display(Name = "Status Badge")]
        public string StatusBadge => (IsDeleted || !IsActive) ? "bg-danger" : "bg-success";

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

    // ---------- FORM VIEW MODEL (Create/Edit) ----------
    // ✅ Unified ViewModel for both Create and Edit
    public class VehicleBrandFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Brand name is required")]
        [StringLength(100, ErrorMessage = "Brand name cannot exceed 100 characters")]
        [Display(Name = "Brand Name")]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // ✅ Added to support the checkbox in Create/Edit views
        [Display(Name = "Is Deleted")]
        public bool IsDeleted { get; set; } = false;
    }

    // ---------- CREATE VIEW MODEL ----------
    // (Optional) If you need a separate create-only model without the Id,
    // but the same form with Id=0 works fine.
    public class VehicleBrandCreateViewModel
    {
        [Required(ErrorMessage = "Brand name is required")]
        [StringLength(100, ErrorMessage = "Brand name cannot exceed 100 characters")]
        [Display(Name = "Brand Name")]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // You can add IsDeleted here as well if you want, but for simplicity
        // we use the unified form view model for both.
    }

    // ---------- UPDATE VIEW MODEL ----------
    // Similarly, can be replaced by the unified form view model.
    public class VehicleBrandUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Brand name is required")]
        [StringLength(100, ErrorMessage = "Brand name cannot exceed 100 characters")]
        [Display(Name = "Brand Name")]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Country cannot exceed 100 characters")]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // Again, add IsDeleted if needed.
    }

    // ---------- SEARCH VIEW MODEL ----------
    public class VehicleBrandSearchViewModel
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Include Deleted")]
        public bool? IncludeDeleted { get; set; }
    }

    // ---------- STATISTICS VIEW MODEL ----------
    public class VehicleBrandStatisticsViewModel
    {
        [Display(Name = "Total Brands")]
        public int TotalBrands { get; set; }

        [Display(Name = "Active Brands")]
        public int ActiveBrands { get; set; }

        [Display(Name = "Inactive Brands")]
        public int DeactiveBrands { get; set; }

        [Display(Name = "Total Vehicles")]
        public int TotalVehicles { get; set; }

        [Display(Name = "Brand with Most Vehicles")]
        public string? BrandWithMostVehicles { get; set; }

        [Display(Name = "Average Vehicles per Brand")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AverageVehiclesPerBrand { get; set; }

        public Dictionary<string, int> CountryDistribution { get; set; } = new();
        public Dictionary<string, int> VehicleDistribution { get; set; } = new();
    }

    // ---------- VEHICLE SUMMARY (for brand details) ----------
    public class VehicleSummaryViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; } = string.Empty;

        [Display(Name = "Manufacturer Year")]
        public int? ManufacturerYear { get; set; }

        [Display(Name = "Brand")]
        public string BrandName { get; set; } = string.Empty;

        [Display(Name = "Color")]
        public string? Color { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        [Display(Name = "Status")]
        public string Status => (IsDeleted || !IsActive) ? "Inactive" : "Active";

        [Display(Name = "Status Badge")]
        public string StatusBadge => (IsDeleted || !IsActive) ? "bg-danger" : "bg-success";
    }
}