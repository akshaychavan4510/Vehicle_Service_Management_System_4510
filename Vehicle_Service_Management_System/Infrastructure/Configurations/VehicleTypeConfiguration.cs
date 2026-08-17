using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class VehicleTypeConfiguration : IEntityTypeConfiguration<VehicleType>
    {
        public void Configure(EntityTypeBuilder<VehicleType> builder)
        {
            builder.ToTable("VehicleType");

            builder.HasKey(vt => vt.Id);

            builder.Property(vt => vt.TypeName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(vt => vt.Description)
                .HasMaxLength(500);

            // ✅ Default value: active (IsDeleted = false)
            builder.Property(vt => vt.IsDeleted)
                .HasDefaultValue(false);

            // ✅ Unique index – ignore soft‑deleted rows
            builder.HasIndex(vt => vt.TypeName)
                .IsUnique()
                .HasDatabaseName("IX_VehicleTypes_TypeName")
                .HasFilter("[IsDeleted] = 0");   // SQL Server syntax

            // ✅ Performance index on IsDeleted
            builder.HasIndex(vt => vt.IsDeleted)
                .HasDatabaseName("IX_VehicleTypes_IsDeleted");

            // ✅ Query filter – hides deleted records by default
            // (If you have a global filter in DbContext, remove this line)
            builder.HasQueryFilter(vt => !vt.IsDeleted);
        }
    }
}