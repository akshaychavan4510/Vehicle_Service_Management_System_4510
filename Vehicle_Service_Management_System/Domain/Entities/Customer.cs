using Vehicle_Service_Management_System.Domain.Common;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class Customer : BaseAuditableEntity
    {
        public string FullName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Address { get; set; }

        // Navigation Properties
        public ICollection<Vehicle> Vehicles { get; set; }
            = new List<Vehicle>();

        public ICollection<ServiceBooking> ServiceBookings { get; set; }
            = new List<ServiceBooking>();
    }
}