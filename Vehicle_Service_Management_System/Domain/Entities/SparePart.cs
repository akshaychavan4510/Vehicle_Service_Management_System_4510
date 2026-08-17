// Domain/Entities/SparePart.cs
using Vehicle_Service_Management_System.Domain.Common;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class SparePart : BaseAuditableEntity
    {
        public string PartName { get; set; } = string.Empty;
        public string PartCode { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public int MinimumStock { get; set; }
        public string? Unit { get; set; }

        // ✅ Added – used for active/inactive status
        public bool IsActive { get; set; }

        // Foreign key
        public int SparePartCategoryId { get; set; }

        // Navigation
        public SparePartCategory SparePartCategory { get; set; } = null!;
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
    }
}