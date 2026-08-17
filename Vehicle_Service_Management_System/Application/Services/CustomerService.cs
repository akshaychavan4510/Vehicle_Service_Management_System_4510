using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.ViewModels.Customer;
using Vehicle_Service_Management_System.Domain.Entities;
using Vehicle_Service_Management_System.Domain.Enums;
using Vehicle_Service_Management_System.Infrastructure.Data;

namespace Vehicle_Service_Management_System.Application.Services
{
    public class CustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CustomerService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── Get overall counts ───
        public async Task<(int Total, int Active, int Deleted)> GetCountsAsync()
        {
            var all = await _context.Customers.IgnoreQueryFilters().ToListAsync();
            return (
                Total: all.Count,
                Active: all.Count(c => !c.IsDeleted),
                Deleted: all.Count(c => c.IsDeleted)
            );
        }

        // ─── Active only ──────────────────────────────────────────
        public async Task<List<Customer>> GetAllActiveAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.FullName)
                .ToListAsync();
        }

        // ─── Deleted only ────────────────────────────────────────
        public async Task<List<Customer>> GetAllDeletedAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(c => c.IsDeleted)
                .OrderBy(c => c.FullName)
                .ToListAsync();
        }

        // ─── All (including soft‑deleted) ──────────────────────
        public async Task<List<Customer>> GetAllIncludingDeletedAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .IgnoreQueryFilters()
                .OrderBy(c => c.FullName)
                .ToListAsync();
        }

        // ─── GetPaginatedAsync (with search, filter, and date range) ──────────
        public async Task<(IEnumerable<CustomerListViewModel> Items, int TotalRecords)> GetPaginatedAsync(
            string filter,
            string? search,
            DateTime? fromDate,
            DateTime? toDate,
            int page,
            int pageSize)
        {
            var query = _context.Customers
                .IgnoreQueryFilters()
                .AsNoTracking();

            // Apply filter (status)
            switch (filter)
            {
                case "active":
                    query = query.Where(c => !c.IsDeleted);
                    break;
                case "deleted":
                    query = query.Where(c => c.IsDeleted);
                    break;
                default: // "all"
                    break;
            }

            // Apply search (name, phone, email)
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(c =>
                    EF.Functions.Like(c.FullName, $"%{search}%") ||
                    EF.Functions.Like(c.PhoneNumber, $"%{search}%") ||
                    (c.Email != null && EF.Functions.Like(c.Email, $"%{search}%"))
                );
            }

            // Apply date range
            if (fromDate.HasValue)
                query = query.Where(c => c.CreatedOn >= fromDate.Value.Date);
            if (toDate.HasValue)
                query = query.Where(c => c.CreatedOn <= toDate.Value.Date.AddDays(1).AddTicks(-1)); // end of day

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerListViewModel
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    PhoneNumber = c.PhoneNumber,
                    Email = c.Email,
                    VehicleCount = c.Vehicles.Count(v => !v.IsDeleted),
                    IsDeleted = c.IsDeleted,
                    CreatedOn = c.CreatedOn
                })
                .ToListAsync();

            return (items, totalRecords);
        }

        // ─── Get by ID (including deleted) ─────────────────────
        public async Task<Customer?> GetByIdIncludingDeletedAsync(int id)
        {
            return await _context.Customers
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // ─── Get for Edit ──────────────────────────────────────
        public async Task<CustomerFormViewModel?> GetForEditAsync(int id)
        {
            var customer = await GetByIdIncludingDeletedAsync(id);
            if (customer == null) return null;

            return new CustomerFormViewModel
            {
                Id = customer.Id,
                FullName = customer.FullName,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                Address = customer.Address
            };
        }

        // ─── Get Details ────────────────────────────────────────
        public async Task<CustomerDetailsViewModel?> GetDetailsAsync(int id)
        {
            var customer = await _context.Customers
                .IgnoreQueryFilters()
                .Include(c => c.Vehicles)
                .Include(c => c.ServiceBookings)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null) return null;

            return new CustomerDetailsViewModel
            {
                Id = customer.Id,
                FullName = customer.FullName,
                PhoneNumber = customer.PhoneNumber,
                Email = customer.Email,
                Address = customer.Address,
                IsDeleted = customer.IsDeleted,
                CreatedOn = customer.CreatedOn,
                ModifiedOn = customer.ModifiedOn,
                VehicleCount = customer.Vehicles?.Count(v => !v.IsDeleted) ?? 0,
                ActiveVehicleCount = customer.Vehicles?.Count(v => !v.IsDeleted) ?? 0,
                TotalBookings = customer.ServiceBookings?.Count(sb => !sb.IsDeleted) ?? 0,
                ActiveBookings = customer.ServiceBookings?.Count(sb => !sb.IsDeleted &&
                    (sb.Status == BookingStatus.Pending || sb.Status == BookingStatus.InProgress)) ?? 0
            };
        }

        // ─── Soft delete (Deactivate) ──────────────────────────
        public async Task<bool> DeactivateAsync(int id)
        {
            var customer = await _context.Customers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null) return false;
            if (customer.IsDeleted) return false;

            customer.IsDeleted = true;
            customer.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Restore (Activate) ──────────────────────────────────
        public async Task<bool> ActivateAsync(int id)
        {
            var customer = await _context.Customers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null) return false;
            if (!customer.IsDeleted) return false;

            customer.IsDeleted = false;
            customer.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Create ──────────────────────────────────────────────
        public async Task CreateAsync(CustomerFormViewModel model)
        {
            var customer = new Customer
            {
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                Address = model.Address,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        // ─── Update ──────────────────────────────────────────────
        public async Task UpdateAsync(CustomerFormViewModel model)
        {
            var customer = await _context.Customers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == model.Id)
                ?? throw new KeyNotFoundException("Customer not found.");

            customer.FullName = model.FullName;
            customer.PhoneNumber = model.PhoneNumber;
            customer.Email = model.Email;
            customer.Address = model.Address;
            customer.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}