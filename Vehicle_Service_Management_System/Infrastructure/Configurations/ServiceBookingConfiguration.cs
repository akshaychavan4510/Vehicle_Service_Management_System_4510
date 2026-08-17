using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class ServiceBookingConfiguration : IEntityTypeConfiguration<ServiceBooking>
    {
        public void Configure(EntityTypeBuilder<ServiceBooking> builder)
        {
            builder.ToTable("ServiceBooking", tableBuilder =>
            {
                tableBuilder.UseSqlOutputClause(false);   // ✅ Required for tables with triggers
            });

            builder.HasKey(sb => sb.Id);

            builder.Property(sb => sb.BookingNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(sb => sb.BookingDate).IsRequired();

            builder.Property(sb => sb.ExpectedDeliveryDate).IsRequired(false);

            builder.Property(sb => sb.DeliveryDate).IsRequired(false);

            builder.Property(sb => sb.OdometerReading).IsRequired(false);

            builder.Property(sb => sb.Complaint)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(sb => sb.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(sb => sb.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(sb => sb.Remarks)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(sb => sb.CustomerId).IsRequired();
            builder.Property(sb => sb.VehicleId).IsRequired();

            // ✅ Correct column mapping
            builder.Property(sb => sb.MechanicId)
                .HasColumnName("MechanicId")
                .IsRequired(false);

            // Audit fields
            builder.Property(sb => sb.CreatedOn).IsRequired();
            builder.Property(sb => sb.ModifiedOn).IsRequired(false);
            builder.Property(sb => sb.IsDeleted).IsRequired();
            builder.Property(sb => sb.IsDeleted ).IsRequired();

            // Indexes
            builder.HasIndex(sb => sb.BookingNumber)
                .IsUnique()
                .HasDatabaseName("IX_ServiceBookings_BookingNumber");

            builder.HasIndex(sb => sb.CustomerId)
                .HasDatabaseName("IX_ServiceBookings_CustomerId");

            builder.HasIndex(sb => sb.VehicleId)
                .HasDatabaseName("IX_ServiceBookings_VehicleId");

            builder.HasIndex(sb => sb.MechanicId)
                .HasDatabaseName("IX_ServiceBookings_MechanicId");

            builder.HasIndex(sb => sb.Status)
                .HasDatabaseName("IX_ServiceBookings_Status");

            builder.HasIndex(sb => sb.BookingDate)
                .HasDatabaseName("IX_ServiceBookings_BookingDate");

            // Relationships
            builder.HasOne(sb => sb.Customer)
                .WithMany(c => c.ServiceBookings)
                .HasForeignKey(sb => sb.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sb => sb.Vehicle)
                .WithMany(v => v.ServiceBookings)
                .HasForeignKey(sb => sb.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sb => sb.Mechanic)
                .WithMany(m => m.ServiceBookings)
                .HasForeignKey(sb => sb.MechanicId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sb => sb.JobCard)
                .WithOne(jc => jc.ServiceBooking)
                .HasForeignKey<JobCard>(jc => jc.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(sb => sb.Invoice)
                .WithOne(i => i.ServiceBooking)
                .HasForeignKey<Invoice>(i => i.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(sb => sb.ServiceBookingDetails)
                .WithOne(d => d.ServiceBooking)
                .HasForeignKey(d => d.ServiceBookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasQueryFilter(sb => !sb.IsDeleted);
        }
    }
}