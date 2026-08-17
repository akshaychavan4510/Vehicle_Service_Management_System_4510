using Vehicle_Service_Management_System.Domain.Common;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class Mechanic : BaseAuditableEntity
    {
        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty; // Changed from Phone to PhoneNumber

        public string? Email { get; set; } // Added

        public string? Specialization { get; set; }

        public int ExperienceYears { get; set; } // Added

        public decimal Salary { get; set; } // Added

        public bool IsAvailable { get; set; } = true;

        // Navigation
        public ICollection<ServiceBooking> ServiceBookings { get; set; } = new List<ServiceBooking>();
    }
}