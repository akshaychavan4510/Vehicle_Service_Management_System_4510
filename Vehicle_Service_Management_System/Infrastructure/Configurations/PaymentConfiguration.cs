using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payment");

            // Primary Key
            builder.HasKey(p => p.Id);

            // Properties
            builder.Property(p => p.InvoiceId)
                .IsRequired();

            builder.Property(p => p.PaymentDate)
                .IsRequired();

            builder.Property(p => p.PaymentMode)
                .IsRequired()
                .HasConversion<int>(); // Store enum as int in database

            builder.Property(p => p.AmountPaid)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(p => p.TransactionReference)
                .HasMaxLength(50);

            builder.Property(p => p.Remarks)
                .HasMaxLength(500);

            // Indexes
            builder.HasIndex(p => p.InvoiceId)
                .HasDatabaseName("IX_Payments_InvoiceId");

            builder.HasIndex(p => p.PaymentDate)
                .HasDatabaseName("IX_Payments_PaymentDate");

            builder.HasIndex(p => p.TransactionReference)
                .HasDatabaseName("IX_Payments_TransactionReference");

            // Relationships
            builder.HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            // Query Filter for soft delete
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}