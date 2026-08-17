// Domain/Entities/Vehicle.cs
using Vehicle_Service_Management_System.Domain.Common;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class Vehicle : BaseAuditableEntity
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public int? ManufacturerYear { get; set; }
        public string? Color { get; set; }
        public int FuelType { get; set; } // store enum as int

        public int CustomerId { get; set; }
        public int VehicleTypeId { get; set; }
        public int VehicleBrandId { get; set; }

        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public VehicleType VehicleType { get; set; } = null!;
        public VehicleBrand VehicleBrand { get; set; } = null!;
        public ICollection<ServiceBooking> ServiceBookings { get; set; } = new List<ServiceBooking>();
    }
}