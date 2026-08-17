using Vehicle_Service_Management_System.Domain.Common;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class ServiceBookingDetail : BaseAuditableEntity
    {
        // =========================================
        // Foreign Keys
        // =========================================

        public int ServiceBookingId { get; set; }

        public int ServiceTypeId { get; set; }


        // =========================================
        // Service Pricing
        // =========================================

        public decimal Price { get; set; }

        public decimal Quantity { get; set; } = 1;

        public decimal TotalAmount { get; set; }


        // =========================================
        // Navigation Properties
        // =========================================

        public ServiceBooking ServiceBooking { get; set; } = null!;

        public ServiceType ServiceType { get; set; } = null!;
    }
}