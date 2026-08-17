using Vehicle_Service_Management_System.Domain.Common;
using Vehicle_Service_Management_System.Domain.Enums;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class Payment : BaseAuditableEntity
    {
        public int InvoiceId { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public PaymentMode PaymentMode { get; set; }

        public decimal AmountPaid { get; set; }

        public string? TransactionReference { get; set; }

        public string? Remarks { get; set; }

        // Navigation
        public Invoice Invoice { get; set; } = null!;
    }
}