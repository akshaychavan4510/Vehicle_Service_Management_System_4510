using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Vehicle_Service_Management_System.Application.ViewModels.Customer
{
    // ============================================================
    // FORM VIEW MODEL (Create/Edit)
    // ============================================================
    public class CustomerFormViewModel
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

        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        // ❌ IsDeleted  removed – use soft delete (IsDeleted) instead
    }

    // ============================================================
    // LIST VIEW MODEL
    // ============================================================
    public class CustomerListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        // ✅ Soft delete flag
        public bool IsDeleted { get; set; }

        [Display(Name = "Status Badge")]
        public string StatusBadge => IsDeleted ? "bg-danger" : "bg-success";

        [Display(Name = "Status")]
        public string Status => IsDeleted ? "Deleted" : "Active";

        [Display(Name = "Vehicle Count")]
        public int VehicleCount { get; set; }

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }
    }

    // ============================================================
    // DETAILS VIEW MODEL
    // ============================================================
    public class CustomerDetailsViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }

        // ✅ Soft delete flag
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public int VehicleCount { get; set; }
        public int ActiveVehicleCount { get; set; }
        public int TotalBookings { get; set; }
        public int ActiveBookings { get; set; }

        public string Status => IsDeleted ? "Deleted" : "Active";
        public string StatusBadge => IsDeleted ? "bg-danger" : "bg-success";
    }

    // ============================================================
    // CREATE VIEW MODEL
    // ============================================================
    public class CustomerCreateViewModel
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

        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters")]
        [Display(Name = "Address")]
        public string? Address { get; set; }
    }

    // ============================================================
    // UPDATE VIEW MODEL
    // ============================================================
    public class CustomerUpdateViewModel
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

        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        // ❌ IsDeleted  removed – use soft delete (IsDeleted) instead
    }

    // ============================================================
    // SEARCH VIEW MODEL
    // ============================================================
    public class CustomerSearchViewModel
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        // ✅ Filter by soft delete status
        [Display(Name = "Include Deleted")]
        public bool IncludeDeleted { get; set; } = false;

        [Display(Name = "Has Vehicles")]
        public bool? HasVehicles { get; set; }
    }

    // ============================================================
    // STATISTICS VIEW MODEL
    // ============================================================
    public class CustomerStatisticsViewModel
    {
        [Display(Name = "Total Customers")]
        public int TotalCustomers { get; set; }

        [Display(Name = "Active Customers")]
        public int ActiveCustomers { get; set; }

        [Display(Name = "Deleted Customers")]
        public int DeletedCustomers { get; set; }

        [Display(Name = "Total Vehicles")]
        public int TotalVehicles { get; set; }

        [Display(Name = "Total Bookings")]
        public int TotalBookings { get; set; }

        [Display(Name = "Total Revenue")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Average Vehicles per Customer")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AverageVehiclesPerCustomer { get; set; }

        [Display(Name = "Average Bookings per Customer")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AverageBookingsPerCustomer { get; set; }

        [Display(Name = "Customer with Most Vehicles")]
        public string? CustomerWithMostVehicles { get; set; }
    }

    // ============================================================
    // HELPER VIEW MODELS (placeholders – adapt as needed)
    // ============================================================
    public class VehicleSummaryViewModel
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string StatusBadge { get; set; } = "bg-secondary";
    }

    public class BookingSummaryViewModel
    {
        public int Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime BookingDate { get; set; }
    }
}