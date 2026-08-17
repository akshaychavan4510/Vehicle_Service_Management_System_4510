using Vehicle_Service_Management_System.Domain.Common;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class VehicleBrand : BaseAuditableEntity
    {
        public string BrandName { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? Description { get; set; }

        // Navigation
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}