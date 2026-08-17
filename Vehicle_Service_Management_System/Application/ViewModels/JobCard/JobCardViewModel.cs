using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Vehicle_Service_Management_System.Application.ViewModels.JobCard
{
    // ============================================================
    // LIST VIEW MODEL
    // ============================================================
    public class JobCardListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Job Card #")]
        public string JobCardNumber { get; set; } = string.Empty;

        [Display(Name = "Booking #")]
        public string BookingNumber { get; set; } = string.Empty;

        // ✅ Added for display in Index
        public int BookingId { get; set; }

        [Display(Name = "Customer")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Vehicle")]
        public string VehicleNumber { get; set; } = string.Empty;

        [Display(Name = "Inspection Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? InspectionDate { get; set; }

        [Display(Name = "Estimated Cost")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal EstimatedCost { get; set; }

        [Display(Name = "Actual Cost")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal ActualCost { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        public string StatusBadge => Status switch
        {
            "Pending" => "bg-warning text-dark",
            "InProgress" => "bg-info text-dark",
            "Completed" => "bg-success",
            "Cancelled" => "bg-danger",
            _ => "bg-secondary"
        };

        [Display(Name = "Created")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }
    }

    // ============================================================
    // DETAILS VIEW MODEL
    // ============================================================
    public class JobCardDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Job Card #")]
        public string JobCardNumber { get; set; } = string.Empty;

        [Display(Name = "Booking #")]
        public string BookingNumber { get; set; } = string.Empty;

        // ✅ Added for Edit page
        public int BookingId { get; set; }

        [Display(Name = "Customer")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Vehicle")]
        public string VehicleRegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Mechanic")]
        public string MechanicName { get; set; } = string.Empty;

        [Display(Name = "Services")]
        public string ServicesSummary { get; set; } = string.Empty;

        // ✅ Computed property for display (Total = Est + Actual)
        public decimal Total => EstimatedCost + ActualCost;

        [Display(Name = "Inspection Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? InspectionDate { get; set; }

        [Display(Name = "Checklist")]
        public string? Checklist { get; set; }

        [Display(Name = "Mechanic Notes")]
        public string? MechanicNotes { get; set; }

        [Display(Name = "Work Performed")]
        public string? WorkPerformed { get; set; }

        [Display(Name = "Estimated Cost")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal EstimatedCost { get; set; }

        [Display(Name = "Actual Cost")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal ActualCost { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        public string StatusBadge => Status switch
        {
            "Pending" => "bg-warning text-dark",
            "InProgress" => "bg-info text-dark",
            "Completed" => "bg-success",
            "Cancelled" => "bg-danger",
            _ => "bg-secondary"
        };

        [Display(Name = "Created")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Modified")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? ModifiedOn { get; set; }
    }

    // ============================================================
    // CREATE VIEW MODEL
    // ============================================================
    public class JobCardCreateViewModel
    {
        [Required(ErrorMessage = "Booking is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a booking")]
        [Display(Name = "Booking")]
        public int BookingId { get; set; }

        [Display(Name = "Inspection Date")]
        [DataType(DataType.DateTime)]
        public DateTime? InspectionDate { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "Checklist cannot exceed 500 characters")]
        [Display(Name = "Checklist")]
        public string? Checklist { get; set; }

        [StringLength(1000, ErrorMessage = "Mechanic Notes cannot exceed 1000 characters")]
        [Display(Name = "Mechanic Notes")]
        public string? MechanicNotes { get; set; }

        [StringLength(1000, ErrorMessage = "Work Performed cannot exceed 1000 characters")]
        [Display(Name = "Work Performed")]
        public string? WorkPerformed { get; set; }

        [Required(ErrorMessage = "Estimated cost is required")]
        [Range(0, 99999999.99, ErrorMessage = "Estimated cost must be between 0 and 99,999,999.99")]
        [Display(Name = "Estimated Cost")]
        [DataType(DataType.Currency)]
        public decimal EstimatedCost { get; set; }

        // ✅ Added: optional Actual Cost
        [Range(0, 99999999.99, ErrorMessage = "Actual cost must be between 0 and 99,999,999.99")]
        [Display(Name = "Actual Cost")]
        public decimal? ActualCost { get; set; }

        // Dropdown
        public List<SelectListItem> AvailableBookings { get; set; } = new();
    }

    // ============================================================
    // UPDATE VIEW MODEL
    // ============================================================
    public class JobCardUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "Job Card #")]
        public string? JobCardNumber { get; set; }

        [Display(Name = "Booking")]
        public int BookingId { get; set; }

        [Display(Name = "Inspection Date")]
        [DataType(DataType.DateTime)]
        public DateTime? InspectionDate { get; set; }

        [StringLength(500, ErrorMessage = "Checklist cannot exceed 500 characters")]
        [Display(Name = "Checklist")]
        public string? Checklist { get; set; }

        [StringLength(1000, ErrorMessage = "Mechanic Notes cannot exceed 1000 characters")]
        [Display(Name = "Mechanic Notes")]
        public string? MechanicNotes { get; set; }

        [StringLength(1000, ErrorMessage = "Work Performed cannot exceed 1000 characters")]
        [Display(Name = "Work Performed")]
        public string? WorkPerformed { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "Estimated cost must be between 0 and 99,999,999.99")]
        [Display(Name = "Estimated Cost")]
        public decimal? EstimatedCost { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "Actual cost must be between 0 and 99,999,999.99")]
        [Display(Name = "Actual Cost")]
        public decimal? ActualCost { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        // Dropdowns
        public List<SelectListItem> AvailableStatuses { get; set; } = new();
        public List<SelectListItem> AvailableBookings { get; set; } = new();
    }

    // ============================================================
    // STATUS UPDATE VIEW MODEL
    // ============================================================
    public class JobCardStatusUpdateViewModel
    {
        [Required(ErrorMessage = "Job Card ID is required")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [StringLength(1000, ErrorMessage = "Work Performed cannot exceed 1000 characters")]
        [Display(Name = "Work Performed")]
        public string? WorkPerformed { get; set; }

        [Range(0, 99999999.99, ErrorMessage = "Actual cost must be between 0 and 99,999,999.99")]
        [Display(Name = "Actual Cost")]
        public decimal? ActualCost { get; set; }
    }

    // ============================================================
    // SEARCH VIEW MODEL
    // ============================================================
    public class JobCardSearchViewModel
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Status")]
        public string? Status { get; set; }

        [Display(Name = "Date From")]
        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Date To")]
        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Display(Name = "Vehicle")]
        public int? VehicleId { get; set; }

        [Display(Name = "Min Cost")]
        public decimal? MinCost { get; set; }

        [Display(Name = "Max Cost")]
        public decimal? MaxCost { get; set; }

        public List<SelectListItem> Statuses { get; set; } = new();
        public List<SelectListItem> Customers { get; set; } = new();
        public List<SelectListItem> Vehicles { get; set; } = new();
    }

    // ============================================================
    // STATISTICS VIEW MODEL
    // ============================================================
    public class JobCardStatisticsViewModel
    {
        [Display(Name = "Total Job Cards")]
        public int TotalJobCards { get; set; }

        [Display(Name = "Pending")]
        public int Pending { get; set; }

        [Display(Name = "In Progress")]
        public int InProgress { get; set; }

        [Display(Name = "Completed")]
        public int Completed { get; set; }

        [Display(Name = "Cancelled")]
        public int Cancelled { get; set; }

        [Display(Name = "Total Estimated Cost")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalEstimatedCost { get; set; }

        [Display(Name = "Total Actual Cost")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalActualCost { get; set; }

        [Display(Name = "Average Estimated Cost")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AverageEstimatedCost { get; set; }

        [Display(Name = "Average Actual Cost")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AverageActualCost { get; set; }

        [Display(Name = "Today's Job Cards")]
        public int TodaysJobCards { get; set; }

        [Display(Name = "Completion Rate")]
        [DisplayFormat(DataFormatString = "{0:F1}%")]
        public decimal CompletionRate { get; set; }

        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, int> MechanicWorkload { get; set; } = new();
    }

    // ============================================================
    // PRINT VIEW MODEL
    // ============================================================
    public class JobCardPrintViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Job Card #")]
        public string JobCardNumber { get; set; } = string.Empty;

        [Display(Name = "Booking #")]
        public string BookingNumber { get; set; } = string.Empty;

        [Display(Name = "Customer")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Address")]
        public string? CustomerAddress { get; set; }

        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Vehicle")]
        public string VehicleRegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Vehicle Model")]
        public string VehicleModel { get; set; } = string.Empty;

        [Display(Name = "Mechanic")]
        public string MechanicName { get; set; } = string.Empty;

        // ✅ Replace ServiceTypeName with ServicesSummary
        [Display(Name = "Services")]
        public string ServicesSummary { get; set; } = string.Empty;

        [Display(Name = "Inspection Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? InspectionDate { get; set; }

        [Display(Name = "Checklist")]
        public string? Checklist { get; set; }

        [Display(Name = "Mechanic Notes")]
        public string? MechanicNotes { get; set; }

        [Display(Name = "Work Performed")]
        public string? WorkPerformed { get; set; }

        [Display(Name = "Estimated Cost")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal EstimatedCost { get; set; }

        [Display(Name = "Actual Cost")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal ActualCost { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Created")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Printed On")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime PrintedOn { get; set; } = DateTime.Now;
    }
}