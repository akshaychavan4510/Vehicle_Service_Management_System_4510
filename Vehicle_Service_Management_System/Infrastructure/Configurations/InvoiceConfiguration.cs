using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            // ─── Table ───
            builder.ToTable("Invoice");

            // ─── Primary Key ───
            builder.HasKey(i => i.Id);

            // ─── Properties ───
            builder.Property(i => i.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(i => i.BookingId)
                .IsRequired();

            builder.Property(i => i.LabourCharge)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(i => i.SparePartsTotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(i => i.GSTPercentage)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            builder.Property(i => i.GSTAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(i => i.Discount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            builder.Property(i => i.GrandTotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(i => i.Remarks)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(i => i.IsPaid)
                .IsRequired()
                .HasDefaultValue(false);

            // Optional: audit properties (inherited from BaseAuditableEntity)
            // builder.Property(i => i.CreatedOn).IsRequired();
            // builder.Property(i => i.ModifiedOn).IsRequired(false);
            // builder.Property(i => i.IsDeleted).IsRequired();
            // builder.Property(i => i.IsDeleted ).IsRequired();

            // ─── Indexes ───
            builder.HasIndex(i => i.InvoiceNumber)
                .IsUnique()
                .HasDatabaseName("IX_Invoices_InvoiceNumber");

            builder.HasIndex(i => i.BookingId)
                .IsUnique()
                .HasDatabaseName("IX_Invoices_BookingId");

            builder.HasIndex(i => i.IsPaid)
                .HasDatabaseName("IX_Invoices_IsPaid");

            // ─── Relationships ───
            builder.HasOne(i => i.ServiceBooking)
                .WithOne(sb => sb.Invoice)
                .HasForeignKey<Invoice>(i => i.BookingId)
                .OnDelete(DeleteBehavior.Restrict); // matches your FK constraint

            // ─── Soft‑Delete Filter ───
            builder.HasQueryFilter(i => !i.IsDeleted);
        }
    }
}