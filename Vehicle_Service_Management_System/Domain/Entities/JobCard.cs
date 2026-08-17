using Vehicle_Service_Management_System.Domain.Common;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class JobCard : BaseAuditableEntity
    {
        public string JobCardNumber { get; set; } = string.Empty;

        public int BookingId { get; set; }

        public DateTime InspectionDate { get; set; }

        public string? Checklist { get; set; }

        // Matches SQL Server column: MechanicNotes
        public string? MechanicNotes { get; set; }

        public string? WorkPerformed { get; set; }

        public decimal EstimatedCost { get; set; }

        public decimal ActualCost { get; set; }

        public string Status { get; set; } = "Pending";

        // Navigation
        public ServiceBooking ServiceBooking { get; set; } = null!;
    }
}