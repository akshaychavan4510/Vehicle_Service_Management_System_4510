using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class ServiceBookingDetailConfiguration
        : IEntityTypeConfiguration<ServiceBookingDetail>
    {
        public void Configure(EntityTypeBuilder<ServiceBookingDetail> builder)
        {
            builder.ToTable("ServiceBookingDetail", tableBuilder =>
            {
                tableBuilder.UseSqlOutputClause(false);   // ✅ Required for triggers
            });

            builder.HasKey(d => d.Id);

            builder.Property(d => d.ServiceBookingId).IsRequired();

            // ✅ Correct column mapping
            builder.Property(d => d.ServiceTypeId)
                .HasColumnName("ServiceTypeId")
                .IsRequired();

            builder.Property(d => d.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(d => d.Quantity)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(d => d.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Relationships
            builder.HasOne(d => d.ServiceBooking)
                .WithMany(sb => sb.ServiceBookingDetails)
                .HasForeignKey(d => d.ServiceBookingId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.ServiceType)
                .WithMany(st => st.ServiceBookingDetails)
                .HasForeignKey(d => d.ServiceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(d => d.ServiceBookingId)
                .HasDatabaseName("IX_ServiceBookingDetail_ServiceBookingId");

            builder.HasIndex(d => d.ServiceTypeId)
                .HasDatabaseName("IX_ServiceBookingDetail_ServiceTypeId");

            builder.HasQueryFilter(d => !d.IsDeleted);
        }
    }
}