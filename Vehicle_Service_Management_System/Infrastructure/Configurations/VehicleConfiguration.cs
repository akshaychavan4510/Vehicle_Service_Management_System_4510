using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
    {
        public void Configure(EntityTypeBuilder<Vehicle> builder)
        {
            builder.ToTable("Vehicle");

            // Primary Key
            builder.HasKey(v => v.Id);

            // Properties
            builder.Property(v => v.RegistrationNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(v => v.VehicleName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(v => v.ManufacturerYear)
                .IsRequired(false);

            builder.Property(v => v.Color)
                .HasMaxLength(30);

            builder.Property(v => v.FuelType)
                .IsRequired()
                .HasConversion<int>(); // enum stored as int

            builder.Property(v => v.CustomerId).IsRequired();
            builder.Property(v => v.VehicleTypeId).IsRequired();
            builder.Property(v => v.VehicleBrandId).IsRequired();

            // ✅ Unique RegistrationNumber – ignore soft‑deleted rows
            builder.HasIndex(v => v.RegistrationNumber)
                .IsUnique()
                .HasDatabaseName("IX_Vehicles_RegistrationNumber")
                .HasFilter("[IsDeleted] = 0");

            // Indexes for foreign keys
            builder.HasIndex(v => v.CustomerId)
                .HasDatabaseName("IX_Vehicles_CustomerId");

            builder.HasIndex(v => v.VehicleTypeId)
                .HasDatabaseName("IX_Vehicles_VehicleTypeId");

            builder.HasIndex(v => v.VehicleBrandId)
                .HasDatabaseName("IX_Vehicles_VehicleBrandId");

            // Optional: index on IsDeleted for performance when filtering active records
            builder.HasIndex(v => v.IsDeleted)
                .HasDatabaseName("IX_Vehicles_IsDeleted");

            // Relationships
            builder.HasOne(v => v.Customer)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.VehicleType)
                .WithMany(vt => vt.Vehicles)
                .HasForeignKey(v => v.VehicleTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(v => v.VehicleBrand)
                .WithMany(vb => vb.Vehicles)
                .HasForeignKey(v => v.VehicleBrandId)
                .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete – excludes deleted records by default
            builder.HasQueryFilter(v => !v.IsDeleted);
        }
    }
}