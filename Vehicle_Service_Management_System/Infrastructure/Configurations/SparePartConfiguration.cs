using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class SparePartConfiguration : IEntityTypeConfiguration<SparePart>
    {
        public void Configure(EntityTypeBuilder<SparePart> builder)
        {
            builder.ToTable("SparePart");

            // Primary Key
            builder.HasKey(sp => sp.Id);

            // Properties
            builder.Property(sp => sp.PartName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sp => sp.PartCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(sp => sp.Brand)
                .HasMaxLength(50);

            builder.Property(sp => sp.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(sp => sp.StockQuantity)
                .IsRequired();

            builder.Property(sp => sp.MinimumStock)
                .IsRequired();

            builder.Property(sp => sp.Unit)
                .HasMaxLength(20);

            // ✅ Inherited properties – set database defaults
            // IsDeleted: default false (not deleted)
            builder.Property(sp => sp.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);   // CORRECTED

            // IsActive: default true (active)
            builder.Property(sp => sp.IsActive)
                .IsRequired()
                .HasDefaultValue(true);    // ADDED

            builder.Property(sp => sp.SparePartCategoryId)
                .IsRequired();

            // Indexes
            builder.HasIndex(sp => sp.PartCode)
                .IsUnique()
                .HasDatabaseName("IX_SpareParts_PartCode");

            builder.HasIndex(sp => sp.PartName)
                .HasDatabaseName("IX_SpareParts_PartName");

            builder.HasIndex(sp => sp.SparePartCategoryId)
                .HasDatabaseName("IX_SpareParts_SparePartCategoryId");

            builder.HasIndex(sp => sp.StockQuantity)
                .HasDatabaseName("IX_SpareParts_StockQuantity");

            // Relationships
            builder.HasOne(sp => sp.SparePartCategory)
                .WithMany(c => c.SpareParts)
                .HasForeignKey(sp => sp.SparePartCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Query Filter for soft delete – hides deleted records
            builder.HasQueryFilter(sp => !sp.IsDeleted);
        }
    }
}