using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.ViewModels.Common;
using Vehicle_Service_Management_System.Application.ViewModels.ServiceType;
using Vehicle_Service_Management_System.Domain.Entities;
using Vehicle_Service_Management_System.Domain.Enums;
using Vehicle_Service_Management_System.Infrastructure.Data;

namespace Vehicle_Service_Management_System.Application.Services
{
    public class ServiceTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ServiceTypeService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ─── Paged List with status filter ───
        public async Task<PagedResult<ServiceTypeListViewModel>> GetPagedAsync(
            string? searchTerm = null,
            bool? includeDeleted = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.ServiceTypes
                .IgnoreQueryFilters()
                .Include(st => st.ServiceBookingDetails)
                .AsQueryable();

            if (includeDeleted == false)
                query = query.Where(st => !st.IsDeleted && st.IsActive);
            else if (includeDeleted == true)
                query = query.Where(st => st.IsDeleted && !st.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(st =>
                    st.ServiceName.Contains(searchTerm) ||
                    (st.Description != null && st.Description.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(st => st.ServiceName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ServiceTypeListViewModel>
            {
                Items = _mapper.Map<List<ServiceTypeListViewModel>>(items),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // ─── Get by Id (active only) ───
        public async Task<ServiceTypeDetailsViewModel?> GetByIdAsync(int id)
        {
            var entity = await _context.ServiceTypes
                .Include(st => st.ServiceBookingDetails)
                    .ThenInclude(sbd => sbd.ServiceBooking)
                .FirstOrDefaultAsync(st => st.Id == id && !st.IsDeleted && st.IsActive);

            return entity == null ? null : _mapper.Map<ServiceTypeDetailsViewModel>(entity);
        }

        // ─── Get for Edit (active only) ───
        public async Task<ServiceTypeFormViewModel?> GetForEditAsync(int id)
        {
            var entity = await _context.ServiceTypes
                .FirstOrDefaultAsync(st => st.Id == id && !st.IsDeleted && st.IsActive);
            return entity == null ? null : _mapper.Map<ServiceTypeFormViewModel>(entity);
        }

        // ─── Create ───
        public async Task<int> CreateAsync(ServiceTypeFormViewModel model)
        {
            if (!await IsServiceNameUniqueAsync(model.ServiceName))
                throw new InvalidOperationException($"Service '{model.ServiceName}' already exists.");

            var entity = _mapper.Map<ServiceType>(model);
            entity.IsDeleted = model.IsDeleted;
            entity.IsActive = !model.IsDeleted;
            entity.CreatedOn = DateTime.UtcNow;

            _context.ServiceTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        // ─── Update ───
        public async Task UpdateAsync(ServiceTypeFormViewModel model)
        {
            var entity = await _context.ServiceTypes
                .FirstOrDefaultAsync(st => st.Id == model.Id && !st.IsDeleted && st.IsActive)
                ?? throw new KeyNotFoundException("Service type not found.");

            if (!await IsServiceNameUniqueAsync(model.ServiceName, model.Id))
                throw new InvalidOperationException($"Service '{model.ServiceName}' already exists.");

            _mapper.Map(model, entity);
            entity.IsDeleted = model.IsDeleted;
            entity.IsActive = !model.IsDeleted;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ─── Uniqueness (ignores soft‑deleted) ───
        public async Task<bool> IsServiceNameUniqueAsync(string serviceName, int? excludeId = null)
        {
            var query = _context.ServiceTypes
                .Where(st => st.ServiceName.ToLower() == serviceName.ToLower() && !st.IsDeleted && st.IsActive);

            if (excludeId.HasValue)
                query = query.Where(st => st.Id != excludeId.Value);

            return !await query.AnyAsync();
        }

        // ─── Soft Delete ───
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _context.ServiceTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(st => st.Id == id && !st.IsDeleted && st.IsActive);

            if (entity == null) return false;

            bool hasActiveBookings = await _context.ServiceBookingDetails
                .IgnoreQueryFilters()
                .AnyAsync(d => d.ServiceTypeId == id &&
                               !d.IsDeleted &&
                               d.ServiceBooking != null &&
                               !d.ServiceBooking.IsDeleted);

            if (hasActiveBookings)
                throw new InvalidOperationException("Cannot deactivate a service type that is used in active bookings.");

            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Restore ───
        public async Task<bool> RestoreAsync(int id)
        {
            var entity = await _context.ServiceTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(st => st.Id == id && st.IsDeleted && !st.IsActive);

            if (entity == null) return false;

            entity.IsDeleted = false;
            entity.IsActive = true;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Hard Delete ───
        public async Task<bool> HardDeleteAsync(int id)
        {
            var entity = await _context.ServiceTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(st => st.Id == id);

            if (entity == null) return false;

            _context.ServiceTypes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Get Deleted List ───
        public async Task<List<ServiceTypeListViewModel>> GetDeletedAsync()
        {
            var items = await _context.ServiceTypes
                .IgnoreQueryFilters()
                .Where(st => st.IsDeleted && !st.IsActive)
                .OrderBy(st => st.ServiceName)
                .ToListAsync();
            return _mapper.Map<List<ServiceTypeListViewModel>>(items);
        }

        // ─── Get Active Dropdown ───
        public async Task<Dictionary<int, string>> GetDropdownAsync()
        {
            return await _context.ServiceTypes
                .Where(st => !st.IsDeleted && st.IsActive)
                .OrderBy(st => st.ServiceName)
                .ToDictionaryAsync(st => st.Id, st => st.ServiceName);
        }

        // ─── Get Statistics (FIXED) ───
        public async Task<ServiceTypeStatisticsViewModel> GetStatisticsAsync()
        {
            var allServices = await _context.ServiceTypes
                .IgnoreQueryFilters()
                .Include(st => st.ServiceBookingDetails)
                .ToListAsync();

            var activeServices = allServices.Where(st => !st.IsDeleted && st.IsActive).ToList();

            var stats = new ServiceTypeStatisticsViewModel
            {
                TotalServices = allServices.Count,
                ActiveServices = activeServices.Count,
                InactiveServices = allServices.Count(st => st.IsDeleted && !st.IsActive),
                TotalBookings = allServices.Sum(st => st.ServiceBookingDetails.Count(d => !d.IsDeleted)),
                TotalRevenue = allServices
                    .SelectMany(st => st.ServiceBookingDetails)
                    .Where(d => !d.IsDeleted && d.ServiceBooking != null && d.ServiceBooking.Status == BookingStatus.Completed)
                    .Sum(d => d.TotalAmount)
            };

            if (activeServices.Any())
            {
                stats.AverageLabourCharge = activeServices.Average(st => st.LabourCharge);
                stats.MaxLabourCharge = activeServices.Max(st => st.LabourCharge);
                stats.MinLabourCharge = activeServices.Min(st => st.LabourCharge);

                // ✅ FIX: Group by ServiceName to avoid duplicate dictionary keys
                stats.MostUsedServices = activeServices
                    .GroupBy(st => st.ServiceName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Sum(st => st.ServiceBookingDetails.Count(d => !d.IsDeleted))
                    );
            }

            return stats;
        }
    }
}