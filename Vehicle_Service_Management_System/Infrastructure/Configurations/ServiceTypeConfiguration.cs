using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class ServiceTypeConfiguration : IEntityTypeConfiguration<ServiceType>
    {
        public void Configure(EntityTypeBuilder<ServiceType> builder)
        {
            builder.ToTable("ServiceType");

            builder.HasKey(st => st.Id);

            // ─── Properties ───
            builder.Property(st => st.ServiceName)
                .HasMaxLength(150)
                .IsRequired();

            builder.Property(st => st.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(st => st.LabourCharge)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(st => st.EstimatedHours)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // ✅ Explicit default for IsDeleted (active by default)
            builder.Property(st => st.IsDeleted)
                .HasDefaultValue(false);

            // ─── Unique index – ignore soft‑deleted rows ───
            builder.HasIndex(st => st.ServiceName)
                .IsUnique()
                .HasDatabaseName("IX_ServiceType_ServiceName")
                .HasFilter("[IsDeleted] = 0");   // SQL Server syntax

            // ─── Performance index on IsDeleted ───
            builder.HasIndex(st => st.IsDeleted)
                .HasDatabaseName("IX_ServiceType_IsDeleted");

            // ─── Relationships ───
            builder.HasMany(st => st.ServiceBookingDetails)
                .WithOne(d => d.ServiceType)
                .HasForeignKey(d => d.ServiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ─── Soft‑delete query filter ───
            // ✅ Keep this if you are NOT using a global filter in DbContext.
            // If you ARE using a global filter, REMOVE this line to avoid duplication.
            builder.HasQueryFilter(st => !st.IsDeleted);
        }
    }
}