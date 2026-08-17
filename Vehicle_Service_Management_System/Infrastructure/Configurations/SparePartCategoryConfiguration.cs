// Infrastructure/Configurations/SparePartCategoryConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class SparePartCategoryConfiguration : IEntityTypeConfiguration<SparePartCategory>
    {
        public void Configure(EntityTypeBuilder<SparePartCategory> builder)
        {
            builder.ToTable("SparePartCategory");

            builder.HasKey(spc => spc.Id);

            builder.Property(spc => spc.CategoryName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(spc => spc.Description)
                .HasMaxLength(500);

            // -------------------- CRITICAL FIXES --------------------
            // IsDeleted – default false (active, not deleted)
            builder.Property(spc => spc.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);   // <-- CHANGED

            // IsActive – default true (active)
            builder.Property(spc => spc.IsActive)
                .IsRequired()
                .HasDefaultValue(true);    // <-- ADDED

            // Optional: CreatedOn / ModifiedOn defaults (if needed)
            // builder.Property(spc => spc.CreatedOn).HasDefaultValueSql("GETUTCDATE()");

            // Index
            builder.HasIndex(spc => spc.CategoryName)
                .IsUnique()
                .HasDatabaseName("IX_SparePartCategories_CategoryName");

            // Soft‑delete filter – hides deleted categories
            builder.HasQueryFilter(spc => !spc.IsDeleted);
        }
    }
}