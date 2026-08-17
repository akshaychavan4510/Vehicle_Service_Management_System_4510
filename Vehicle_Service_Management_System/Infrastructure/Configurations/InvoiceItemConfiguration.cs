using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Infrastructure.Configurations
{
    public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
    {
        public void Configure(EntityTypeBuilder<InvoiceItem> builder)
        {
            // Table name
            builder.ToTable("InvoiceItem");

            // Primary Key
            builder.HasKey(ii => ii.Id);

            // ─── Properties ───
            builder.Property(ii => ii.InvoiceId)
                .IsRequired();

            builder.Property(ii => ii.SparePartId)
                .IsRequired();

            builder.Property(ii => ii.Quantity)
                .IsRequired();

            builder.Property(ii => ii.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(ii => ii.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            // Audit properties (inherited from BaseAuditableEntity)
            builder.Property(ii => ii.CreatedOn)
                .IsRequired();

            builder.Property(ii => ii.ModifiedOn)
                .IsRequired(false);

            builder.Property(ii => ii.IsDeleted)
                .IsRequired();

            builder.Property(ii => ii.IsDeleted )
                .IsRequired();

            // ─── Indexes ───
            builder.HasIndex(ii => ii.InvoiceId)
                .HasDatabaseName("IX_InvoiceItems_InvoiceId");

            builder.HasIndex(ii => ii.SparePartId)
                .HasDatabaseName("IX_InvoiceItems_SparePartId");

            // ─── Relationships ───
            // Invoice → InvoiceItems (one‑to‑many)
            builder.HasOne(ii => ii.Invoice)
                .WithMany(inv => inv.InvoiceItems)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade); // delete invoice → delete its items

            // SparePart → InvoiceItems (one‑to‑many)
            builder.HasOne(ii => ii.SparePart)
                .WithMany(sp => sp.InvoiceItems)
                .HasForeignKey(ii => ii.SparePartId)
                .OnDelete(DeleteBehavior.Restrict); // prevent deleting a part that has sales history

            // ─── Soft‑Delete Filter ───
            // Automatically exclude soft‑deleted items from all queries
            builder.HasQueryFilter(ii => !ii.IsDeleted);
        }
    }
}