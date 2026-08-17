using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class JobCardConfiguration : IEntityTypeConfiguration<JobCard>
    {
        public void Configure(EntityTypeBuilder<JobCard> builder)
        {
            // =====================================================
            // Table
            // =====================================================

            builder.ToTable("JobCard");

            // =====================================================
            // Primary Key
            // =====================================================

            builder.HasKey(jc => jc.Id);

            // =====================================================
            // Properties
            // =====================================================

            builder.Property(jc => jc.JobCardNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(jc => jc.BookingId)
                .IsRequired();

            builder.Property(jc => jc.InspectionDate)
                .IsRequired();

            builder.Property(jc => jc.Checklist)
                .HasMaxLength(500)
                .IsRequired(false);

            // IMPORTANT:
            // SQL Server column is "MechanicNotes"
            // NOT "MechanicsNotes"
            builder.Property(jc => jc.MechanicNotes)
                .HasColumnName("MechanicNotes")
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(jc => jc.WorkPerformed)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(jc => jc.EstimatedCost)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(jc => jc.ActualCost)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(jc => jc.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            // =====================================================
            // Indexes
            // =====================================================

            builder.HasIndex(jc => jc.JobCardNumber)
                .IsUnique()
                .HasDatabaseName("IX_JobCards_JobCardNumber");

            builder.HasIndex(jc => jc.BookingId)
                .IsUnique()
                .HasDatabaseName("IX_JobCards_BookingId");

            builder.HasIndex(jc => jc.Status)
                .HasDatabaseName("IX_JobCards_Status");

            // =====================================================
            // Relationship
            // JobCard -> ServiceBooking
            // One Booking -> One JobCard
            // =====================================================

            builder.HasOne(jc => jc.ServiceBooking)
                .WithOne(sb => sb.JobCard)
                .HasForeignKey<JobCard>(jc => jc.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // =====================================================
            // Soft Delete Query Filter
            // =====================================================

            builder.HasQueryFilter(jc => !jc.IsDeleted);
        }
    }
}