using Vehicle_Service_Management_System.Domain.Common;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class Invoice : BaseAuditableEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;

        // Foreign Key
        public int BookingId { get; set; }

        public decimal LabourCharge { get; set; }

        public decimal SparePartsTotal { get; set; }

        public decimal GSTPercentage { get; set; }

        public decimal GSTAmount { get; set; }

        public decimal Discount { get; set; }

        public decimal GrandTotal { get; set; }

        public string? Remarks { get; set; }

        public bool IsPaid { get; set; } = false;

        // Navigation
        public ServiceBooking ServiceBooking { get; set; } = null!;

        public ICollection<InvoiceItem> InvoiceItems { get; set; }
            = new List<InvoiceItem>();

        public ICollection<Payment> Payments { get; set; }
            = new List<Payment>();
    }
}