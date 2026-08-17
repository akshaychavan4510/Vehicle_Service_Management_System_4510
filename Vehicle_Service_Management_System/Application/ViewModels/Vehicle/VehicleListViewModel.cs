using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Vehicle_Service_Management_System.Domain.Enums;

namespace Vehicle_Service_Management_System.Application.ViewModels.Vehicle
{
    // ---------- LIST VIEW MODEL ----------
    public class VehicleListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; } = string.Empty;

        [Display(Name = "Customer")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Vehicle Type")]
        public string VehicleTypeName { get; set; } = string.Empty;

        [Display(Name = "Brand")]
        public string VehicleBrandName { get; set; } = string.Empty;

        [Display(Name = "Manufacturer Year")]
        public int? ManufacturerYear { get; set; }

        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        [Display(Name = "Status")]
        public string Status => (IsDeleted || !IsActive) ? "Deactive" : "Active";

        [Display(Name = "Status Badge")]
        public string StatusBadge => (IsDeleted || !IsActive) ? "bg-danger" : "bg-success";

        [Display(Name = "Total Bookings")]
        public int TotalBookings { get; set; }

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }
    }

    // ---------- FORM VIEW MODEL (Create/Edit) ----------
    public class VehicleFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Registration number is required")]
        [StringLength(20, ErrorMessage = "Registration number cannot exceed 20 characters")]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle name is required")]
        [StringLength(100, ErrorMessage = "Vehicle name cannot exceed 100 characters")]
        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; } = string.Empty;

        [Range(1980, 2100, ErrorMessage = "Manufacturer year must be between 1980 and 2100")]
        [Display(Name = "Manufacturer Year")]
        public int? ManufacturerYear { get; set; }

        [StringLength(30, ErrorMessage = "Color cannot exceed 30 characters")]
        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Required(ErrorMessage = "Fuel type is required")]
        [Display(Name = "Fuel Type")]
        public FuelType FuelType { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Vehicle type is required")]
        [Display(Name = "Vehicle Type")]
        public int VehicleTypeId { get; set; }

        [Required(ErrorMessage = "Vehicle brand is required")]
        [Display(Name = "Vehicle Brand")]
        public int VehicleBrandId { get; set; }

        // ❌ IsDeleted and IsActive are not user-editable – managed by service

        public List<SelectListItem> Customers { get; set; } = new();
        public List<SelectListItem> VehicleTypes { get; set; } = new();
        public List<SelectListItem> VehicleBrands { get; set; } = new();
    }

    // ---------- DETAILS VIEW MODEL ----------
    public class VehicleDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; } = string.Empty;

        [Display(Name = "Customer")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Vehicle Type")]
        public string VehicleTypeName { get; set; } = string.Empty;

        [Display(Name = "Brand")]
        public string VehicleBrandName { get; set; } = string.Empty;

        [Display(Name = "Manufacturer Year")]
        public int? ManufacturerYear { get; set; }

        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Display(Name = "Fuel Type")]
        public string FuelType { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        [Display(Name = "Status")]
        public string Status => (IsDeleted || !IsActive) ? "Deactive" : "Active";

        [Display(Name = "Status Badge")]
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

        [Display(Name = "Total Service Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalServiceAmount { get; set; }

        public int CustomerId { get; set; }
        public int VehicleTypeId { get; set; }
        public int VehicleBrandId { get; set; }
    }

    // ---------- SUMMARY VIEW MODEL ----------
    public class VehicleSummaryViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Vehicle Name")]
        public string VehicleName { get; set; } = string.Empty;

        [Display(Name = "Vehicle Type")]
        public string VehicleTypeName { get; set; } = string.Empty;

        [Display(Name = "Brand")]
        public string VehicleBrandName { get; set; } = string.Empty;

        [Display(Name = "Manufacturer Year")]
        public int? ManufacturerYear { get; set; }

        [Display(Name = "Color")]
        public string? Color { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        [Display(Name = "Status")]
        public string Status => (IsDeleted || !IsActive) ? "Deactive" : "Active";

        [Display(Name = "Status Badge")]
        public string StatusBadge => (IsDeleted || !IsActive) ? "bg-danger" : "bg-success";

        [Display(Name = "Total Bookings")]
        public int TotalBookings { get; set; }
    }

    // ---------- CREATE VIEW MODEL ----------
    public class VehicleCreateViewModel
    {
        [Required(ErrorMessage = "Registration number is required")]
        [StringLength(20, ErrorMessage = "Registration number cannot exceed 20 characters")]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle name is required")]
        [StringLength(100, ErrorMessage = "Vehicle name cannot exceed 100 characters")]
        [Display(Name = "Vehicle Name / Model")]
        public string VehicleName { get; set; } = string.Empty;

        [Range(1980, 2100, ErrorMessage = "Manufacturer year must be between 1980 and 2100")]
        [Display(Name = "Manufacturer Year")]
        public int? ManufacturerYear { get; set; }

        [StringLength(30, ErrorMessage = "Color cannot exceed 30 characters")]
        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Required(ErrorMessage = "Fuel type is required")]
        [Display(Name = "Fuel Type")]
        public FuelType FuelType { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Vehicle type is required")]
        [Display(Name = "Vehicle Type")]
        public int VehicleTypeId { get; set; }

        [Required(ErrorMessage = "Vehicle brand is required")]
        [Display(Name = "Vehicle Brand")]
        public int VehicleBrandId { get; set; }
    }

    // ---------- UPDATE VIEW MODEL ----------
    public class VehicleUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Registration number is required")]
        [StringLength(20, ErrorMessage = "Registration number cannot exceed 20 characters")]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle name is required")]
        [StringLength(100, ErrorMessage = "Vehicle name cannot exceed 100 characters")]
        [Display(Name = "Vehicle Name / Model")]
        public string VehicleName { get; set; } = string.Empty;

        [Range(1980, 2100, ErrorMessage = "Manufacturer year must be between 1980 and 2100")]
        [Display(Name = "Manufacturer Year")]
        public int? ManufacturerYear { get; set; }

        [StringLength(30, ErrorMessage = "Color cannot exceed 30 characters")]
        [Display(Name = "Color")]
        public string? Color { get; set; }

        [Required(ErrorMessage = "Fuel type is required")]
        [Display(Name = "Fuel Type")]
        public FuelType FuelType { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Vehicle type is required")]
        [Display(Name = "Vehicle Type")]
        public int VehicleTypeId { get; set; }

        [Required(ErrorMessage = "Vehicle brand is required")]
        [Display(Name = "Vehicle Brand")]
        public int VehicleBrandId { get; set; }
    }

    // ---------- SEARCH VIEW MODEL ----------
    public class VehicleSearchViewModel
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Display(Name = "Vehicle Type")]
        public int? VehicleTypeId { get; set; }

        [Display(Name = "Vehicle Brand")]
        public int? VehicleBrandId { get; set; }

        [Display(Name = "Fuel Type")]
        public FuelType? FuelType { get; set; }

        [Display(Name = "Manufacturer Year")]
        public int? ManufacturerYear { get; set; }

        [Display(Name = "Include Deleted")]
        public bool? IncludeDeleted { get; set; }

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public List<SelectListItem> Customers { get; set; } = new();
        public List<SelectListItem> VehicleTypes { get; set; } = new();
        public List<SelectListItem> VehicleBrands { get; set; } = new();
        public List<SelectListItem> FuelTypes { get; set; } = new();
    }

    // ---------- STATISTICS VIEW MODEL ----------
    public class VehicleStatisticsViewModel
    {
        [Display(Name = "Total Vehicles")]
        public int TotalVehicles { get; set; }

        [Display(Name = "Active Vehicles")]
        public int ActiveVehicles { get; set; }

        [Display(Name = "Deactive Vehicles")]
        public int DeactiveVehicles { get; set; }

        [Display(Name = "Total Customers with Vehicles")]
        public int TotalCustomers { get; set; }

        [Display(Name = "Average Vehicles per Customer")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AverageVehiclesPerCustomer { get; set; }

        [Display(Name = "Total Service Revenue")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalServiceRevenue { get; set; }

        public Dictionary<string, int> VehicleTypeDistribution { get; set; } = new();
        public Dictionary<string, int> VehicleBrandDistribution { get; set; } = new();
        public Dictionary<string, int> FuelTypeDistribution { get; set; } = new();
    }

    // ---------- VEHICLE BY CUSTOMER VIEW MODEL ----------
    public class VehicleByCustomerViewModel
    {
        public int CustomerId { get; set; }

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Vehicle Count")]
        public int VehicleCount { get; set; }

        public List<VehicleSummaryViewModel> Vehicles { get; set; } = new();
    }
}