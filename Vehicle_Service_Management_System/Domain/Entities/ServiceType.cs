using Vehicle_Service_Management_System.Domain.Common;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class ServiceType : BaseAuditableEntity
    {
        public string ServiceName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal LabourCharge { get; set; }

        public decimal EstimatedHours { get; set; }

        public ICollection<ServiceBookingDetail> ServiceBookingDetails
        {
            get; set;
        } = new List<ServiceBookingDetail>();
    }
}