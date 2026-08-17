using Vehicle_Service_Management_System.Domain.Common;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class InvoiceItem : BaseAuditableEntity
    {
        public int InvoiceId { get; set; }

        public int SparePartId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalAmount { get; set; }

        // Navigation
        public Invoice Invoice { get; set; } = null!;

        public SparePart SparePart { get; set; } = null!;
    }
}