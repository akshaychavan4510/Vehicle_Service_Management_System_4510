using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Vehicle_Service_Management_System.Domain.Enums;

namespace Vehicle_Service_Management_System.Application.ViewModels.ServiceBooking
{
    // 1. LIST
    public class ServiceBookingListViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Booking #")]
        public string BookingNumber { get; set; } = string.Empty;

        [Display(Name = "Booking Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime BookingDate { get; set; }

        [Display(Name = "Expected Delivery")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [Display(Name = "Customer")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Vehicle")]
        public string VehicleRegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Mechanic")]
        public string MechanicName { get; set; } = string.Empty;

        [Display(Name = "Services")]
        public string ServicesSummary { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalAmount { get; set; }

        public bool HasJobCard { get; set; }
        public bool HasInvoice { get; set; }

        [Display(Name = "Created")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }

        public string StatusBadge => Status switch
        {
            "Pending" => "bg-warning",
            "InProgress" => "bg-info",
            "Completed" => "bg-success",
            "Cancelled" => "bg-danger",
            _ => "bg-secondary"
        };
    }

    // 2. CREATE
    public class ServiceBookingCreateViewModel
    {
        [Required(ErrorMessage = "Customer is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        public int VehicleId { get; set; }

        public int? MechanicId { get; set; }

        [Required(ErrorMessage = "Booking date is required")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime? ExpectedDeliveryDate { get; set; }

        public int? OdometerReading { get; set; }

        [StringLength(500)]
        public string? Complaint { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required(ErrorMessage = "At least one service is required")]
        [MinLength(1, ErrorMessage = "Select at least one service")]
        public List<ServiceBookingDetailInputViewModel> Services { get; set; } = new();

        public List<SelectListItem> Customers { get; set; } = new();
        public List<SelectListItem> Vehicles { get; set; } = new();
        public List<SelectListItem> Mechanics { get; set; } = new();
        public List<SelectListItem> ServiceTypes { get; set; } = new();
    }

    // 3. EDIT – corrected (added BookingNumber and TotalAmount)
    public class ServiceBookingUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "Booking Number")]
        public string BookingNumber { get; set; } = string.Empty;

        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Vehicle is required")]
        public int VehicleId { get; set; }

        public int? MechanicId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ExpectedDeliveryDate { get; set; }

        public int? OdometerReading { get; set; }

        [StringLength(500)]
        public string? Complaint { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public BookingStatus Status { get; set; }

        [Required(ErrorMessage = "At least one service is required")]
        [MinLength(1, ErrorMessage = "Select at least one service")]
        public List<ServiceBookingDetailInputViewModel> Services { get; set; } = new();

        public List<SelectListItem> Customers { get; set; } = new();
        public List<SelectListItem> Vehicles { get; set; } = new();
        public List<SelectListItem> Mechanics { get; set; } = new();
        public List<SelectListItem> ServiceTypes { get; set; } = new();
        public List<ServiceTypeOption> ServiceTypeOptions { get; set; } = new();
    }

    // 4. DETAILS
    public class ServiceBookingDetailsViewModel
    {
        public int Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string VehicleRegistrationNumber { get; set; } = string.Empty;
        public string VehicleModel { get; set; } = string.Empty;
        public string MechanicName { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime BookingDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? ExpectedDeliveryDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? DeliveryDate { get; set; }

        public int? OdometerReading { get; set; }
        public string? Complaint { get; set; }
        public string? Remarks { get; set; }
        public string Status { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalAmount { get; set; }

        public List<ServiceBookingDetailLineViewModel> Services { get; set; } = new();
        public bool HasJobCard { get; set; }
        public bool HasInvoice { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedOn { get; set; }

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? ModifiedOn { get; set; }

        public string StatusBadge => Status switch
        {
            "Pending" => "bg-warning",
            "InProgress" => "bg-info",
            "Completed" => "bg-success",
            "Cancelled" => "bg-danger",
            _ => "bg-secondary"
        };
    }

    // 5. STATUS UPDATE
    public class ServiceBookingStatusUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public BookingStatus Status { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DeliveryDate { get; set; }
    }

    // 6. SEARCH
    public class ServiceBookingSearchViewModel
    {
        public string? SearchTerm { get; set; }
        public BookingStatus? Status { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }
        public int? CustomerId { get; set; }
        public int? VehicleId { get; set; }
        public int? MechanicId { get; set; }

        public List<SelectListItem> Customers { get; set; } = new();
        public List<SelectListItem> Vehicles { get; set; } = new();
        public List<SelectListItem> Mechanics { get; set; } = new();
        public List<SelectListItem> ServiceTypes { get; set; } = new();
        public List<SelectListItem> Statuses { get; set; } = new();
    }

    // 7. STATISTICS
    public class ServiceBookingStatisticsViewModel
    {
        public int TotalBookings { get; set; }
        public int Pending { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalRevenue { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AverageBookingValue { get; set; }

        public int TodaysBookings { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TodaysRevenue { get; set; }

        public Dictionary<string, int> ServiceTypeDistribution { get; set; } = new();
        public Dictionary<string, int> MechanicWorkload { get; set; } = new();
    }

    // 8. SUPPORTING VIEW MODELS
    // ✅ FIX: Added Id and TotalAmount as regular property
    public class ServiceBookingDetailInputViewModel
    {
        public int Id { get; set; }   // ✅ Added

        [Required]
        public int ServiceTypeId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 99)]
        public int Quantity { get; set; } = 1;

        // ✅ Regular property (not computed)
        public decimal TotalAmount { get; set; }
    }

    public class ServiceBookingDetailLineViewModel
    {
        public string ServiceName { get; set; } = string.Empty;
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalAmount { get; set; }
    }
    public class ServiceTypeOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal LabourCharge { get; set; }
    }
}