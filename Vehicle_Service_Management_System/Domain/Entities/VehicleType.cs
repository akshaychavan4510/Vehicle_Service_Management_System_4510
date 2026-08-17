using Vehicle_Service_Management_System.Domain.Common;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class VehicleType : BaseAuditableEntity
    {
        public string TypeName { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}