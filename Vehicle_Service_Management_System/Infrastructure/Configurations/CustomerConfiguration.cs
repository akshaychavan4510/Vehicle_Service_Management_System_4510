// Infrastructure/Configurations/CustomerConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("Customer");

            builder.Property(c => c.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Email)
                .HasMaxLength(150);

            builder.Property(c => c.Address)
                .HasMaxLength(250);

            // Unique phone number – but ignore soft‑deleted rows
            builder.HasIndex(c => c.PhoneNumber)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");  // Allows duplicate phone numbers in deleted records

            // Global filter: only return non‑deleted rows
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}