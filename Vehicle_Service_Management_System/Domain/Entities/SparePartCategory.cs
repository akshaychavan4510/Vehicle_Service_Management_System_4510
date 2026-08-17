using Vehicle_Service_Management_System.Domain.Common;

namespace Vehicle_Service_Management_System.Domain.Entities
{
    public class SparePartCategory : BaseAuditableEntity
    {
        // BaseAuditableEntity provides: Id, CreatedOn, ModifiedOn, IsDeleted, IsActive
        // Do NOT redeclare them.

        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Navigation
        public ICollection<SparePart> SpareParts { get; set; } = new List<SparePart>();
    }
}