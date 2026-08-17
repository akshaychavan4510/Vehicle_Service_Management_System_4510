using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class VehicleBrandConfiguration : IEntityTypeConfiguration<VehicleBrand>
    {
        public void Configure(EntityTypeBuilder<VehicleBrand> builder)
        {
            builder.ToTable("VehicleBrand");

            builder.HasKey(vb => vb.Id);

            builder.Property(vb => vb.BrandName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(vb => vb.Country)
                .HasMaxLength(100);

            builder.Property(vb => vb.Description)
                .HasMaxLength(500);

            // ✅ Unique BrandName – ignore soft‑deleted rows
            builder.HasIndex(vb => vb.BrandName)
                .IsUnique()
                .HasDatabaseName("IX_VehicleBrands_BrandName")
                .HasFilter("[IsDeleted] = 0");

            // ✅ Index on IsDeleted for performance
            builder.HasIndex(vb => vb.IsDeleted)
                .HasDatabaseName("IX_VehicleBrands_IsDeleted");

            // ❗ Global query filter is applied in ApplicationDbContext for all BaseAuditableEntity.
            // Do NOT add builder.HasQueryFilter(vb => !vb.IsDeleted) here to avoid duplication.
        }
    }
}