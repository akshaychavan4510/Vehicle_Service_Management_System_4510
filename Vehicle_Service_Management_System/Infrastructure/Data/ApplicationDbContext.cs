// Infrastructure/Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using Vehicle_Service_Management_System.Domain.Common;
using Vehicle_Service_Management_System.Domain.Entities;
using Vehicle_Service_Management_System.Infrastructure.Identity;

namespace Vehicle_Service_Management_System.Infrastructure.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ─── DbSets ───
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Vehicle> Vehicles => Set<Vehicle>();
        public DbSet<VehicleType> VehicleTypes => Set<VehicleType>();
        public DbSet<VehicleBrand> VehicleBrands => Set<VehicleBrand>();
        public DbSet<Mechanic> Mechanics => Set<Mechanic>();
        public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
        public DbSet<ServiceBooking> ServiceBookings => Set<ServiceBooking>();
        public DbSet<ServiceBookingDetail> ServiceBookingDetails => Set<ServiceBookingDetail>();
        public DbSet<JobCard> JobCards => Set<JobCard>();
        public DbSet<SparePartCategory> SparePartCategories => Set<SparePartCategory>();
        public DbSet<SparePart> SpareParts => Set<SparePart>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);  // Identity tables

            // ─── Apply all IEntityTypeConfiguration<T> ───
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // ─── Custom Identity properties ───
            builder.Entity<ApplicationUser>()
                .Property(u => u.FullName)
                .HasMaxLength(150)
                .IsRequired();

            // ─── Enum conversions ───
            builder.Entity<Vehicle>()
                .Property(v => v.FuelType)
                .HasConversion<int>();

            builder.Entity<ServiceBooking>()
                .Property(sb => sb.Status)
                .HasConversion<int>();

            builder.Entity<Payment>()
                .Property(p => p.PaymentMode)
                .HasConversion<int>();

            // ─── Global query filter for ALL soft‑deletable entities ───
            // This is the ONLY place where the filter is applied.
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var filter = BuildFilterExpression(entityType.ClrType);
                    builder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }
        }

        private static LambdaExpression BuildFilterExpression(Type entityType)
        {
            var parameter = Expression.Parameter(entityType, "e");
            var property = Expression.Property(parameter, nameof(BaseAuditableEntity.IsDeleted));
            var notDeleted = Expression.Not(property);
            return Expression.Lambda(notDeleted, parameter);
        }

        // ─── SaveChanges overrides ───
        public override int SaveChanges()
        {
            ApplyAuditAndSoftDelete();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditAndSoftDelete();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditAndSoftDelete()
        {
            foreach (var entry in ChangeTracker.Entries<BaseAuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedOn = DateTime.UtcNow;
                        entry.Entity.IsDeleted = false;
                        entry.Entity.IsActive = true;
                        break;

                    case EntityState.Modified:
                        entry.Entity.ModifiedOn = DateTime.UtcNow;
                        break;

                    case EntityState.Deleted:
                        // Convert physical delete to soft delete
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.IsActive = false;
                        entry.Entity.ModifiedOn = DateTime.UtcNow;
                        break;
                }
            }
        }
    }
}