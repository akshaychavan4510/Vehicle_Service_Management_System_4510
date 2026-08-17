using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class MechanicConfiguration : IEntityTypeConfiguration<Mechanic>
    {
        public void Configure(EntityTypeBuilder<Mechanic> builder)
        {
            builder.ToTable("Mechanic");

            // Primary Key
            builder.HasKey(m => m.Id);

            // Properties
            builder.Property(m => m.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15);

            builder.Property(m => m.Email)
                .HasMaxLength(100);

            builder.Property(m => m.Specialization)
                .HasMaxLength(50);

            builder.Property(m => m.ExperienceYears)
                .IsRequired();

            builder.Property(m => m.Salary)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(m => m.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            builder.HasIndex(m => m.PhoneNumber)
                .IsUnique()
                .HasDatabaseName("IX_Mechanics_PhoneNumber");

            builder.HasIndex(m => m.Email)
                .IsUnique()
                .HasDatabaseName("IX_Mechanics_Email")
                .HasFilter("[Email] IS NOT NULL");

            builder.HasIndex(m => m.IsAvailable)
                .HasDatabaseName("IX_Mechanics_IsAvailable");

            // Query Filter for soft delete
            builder.HasQueryFilter(m => !m.IsDeleted);
        }
    }
}