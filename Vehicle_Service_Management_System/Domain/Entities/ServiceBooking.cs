using Vehicle_Service_Management_System.Domain.Common;
using Vehicle_Service_Management_System.Domain.Enums;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class ServiceBooking : BaseAuditableEntity
    {
        // =========================================
        // Basic Information
        // =========================================

        public string BookingNumber { get; set; } = string.Empty;

        public DateTime BookingDate { get; set; }

        public DateTime? ExpectedDeliveryDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public int? OdometerReading { get; set; }

        public string? Complaint { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public decimal TotalAmount { get; set; }

        public string? Remarks { get; set; }


        // =========================================
        // Foreign Keys
        // =========================================

        public int CustomerId { get; set; }

        public int VehicleId { get; set; }

        public int? MechanicId { get; set; }


        // =========================================
        // Navigation Properties
        // =========================================

        public Customer Customer { get; set; } = null!;

        public Vehicle Vehicle { get; set; } = null!;

        public Mechanic? Mechanic { get; set; }


        // =========================================
        // Related Entities
        // =========================================

        public ICollection<ServiceBookingDetail> ServiceBookingDetails
        {
            get;
            set;
        } = new List<ServiceBookingDetail>();

        public JobCard? JobCard { get; set; }

        public Invoice? Invoice { get; set; }
    }
}