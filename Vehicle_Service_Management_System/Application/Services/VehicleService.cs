using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.ViewModels.Common;
using Vehicle_Service_Management_System.Application.ViewModels.Vehicle;
using Vehicle_Service_Management_System.Domain.Entities;
using Vehicle_Service_Management_System.Domain.Enums;
using Vehicle_Service_Management_System.Infrastructure.Data;

namespace Vehicle_Service_Management_System.Application.Services
{
    public class VehicleService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public VehicleService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region Paged List with Filters & Date Range

        public async Task<PagedResult<VehicleListViewModel>> GetPagedAsync(
            string? search = null,
            int? customerId = null,
            int? vehicleTypeId = null,
            int? vehicleBrandId = null,
            FuelType? fuelType = null,
            bool? includeDeleted = null,  // null = all, false = active only, true = deleted only
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _context.Vehicles
                .IgnoreQueryFilters()
                .Include(v => v.Customer)
                .Include(v => v.VehicleType)
                .Include(v => v.VehicleBrand)
                .Include(v => v.ServiceBookings)
                .AsQueryable();

            // ─── Status filter ───
            if (includeDeleted == false)
                query = query.Where(v => !v.IsDeleted && v.IsActive);
            else if (includeDeleted == true)
                query = query.Where(v => v.IsDeleted && !v.IsActive);

            // ─── Search ───
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(v =>
                    (v.RegistrationNumber ?? "").Contains(search) ||
                    (v.VehicleName ?? "").Contains(search) ||
                    (v.Customer != null && v.Customer.FullName.Contains(search)) ||
                    (v.VehicleType != null && v.VehicleType.TypeName.Contains(search)) ||
                    (v.VehicleBrand != null && v.VehicleBrand.BrandName.Contains(search)));
            }

            // ─── Other filters ───
            if (customerId.HasValue)
                query = query.Where(v => v.CustomerId == customerId.Value);
            if (vehicleTypeId.HasValue)
                query = query.Where(v => v.VehicleTypeId == vehicleTypeId.Value);
            if (vehicleBrandId.HasValue)
                query = query.Where(v => v.VehicleBrandId == vehicleBrandId.Value);
            if (fuelType.HasValue)
                query = query.Where(v => v.FuelType == (int)fuelType.Value);

            // ─── Date range filter ───
            if (fromDate.HasValue)
                query = query.Where(v => v.CreatedOn >= fromDate.Value.Date);
            if (toDate.HasValue)
                query = query.Where(v => v.CreatedOn <= toDate.Value.Date.AddDays(1).AddTicks(-1));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(v => v.RegistrationNumber)
                .ThenBy(v => v.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<VehicleListViewModel>
            {
                Items = _mapper.Map<List<VehicleListViewModel>>(items),
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        #endregion

        #region Non‑Paged Lists (for optional use)

        public async Task<List<Vehicle>> GetAllActiveAsync()
        {
            return await _context.Vehicles
                .AsNoTracking()
                .Include(v => v.Customer)
                .Include(v => v.VehicleType)
                .Include(v => v.VehicleBrand)
                .Include(v => v.ServiceBookings)
                .Where(v => !v.IsDeleted && v.IsActive)
                .OrderByDescending(v => v.Id)
                .ToListAsync();
        }

        public async Task<List<Vehicle>> GetAllDeactivatedAsync()
        {
            return await _context.Vehicles
                .AsNoTracking()
                .Include(v => v.Customer)
                .Include(v => v.VehicleType)
                .Include(v => v.VehicleBrand)
                .Include(v => v.ServiceBookings)
                .IgnoreQueryFilters()
                .Where(v => v.IsDeleted && !v.IsActive)
                .OrderByDescending(v => v.Id)
                .ToListAsync();
        }

        public async Task<List<Vehicle>> GetAllIncludingDeletedAsync()
        {
            return await _context.Vehicles
                .AsNoTracking()
                .Include(v => v.Customer)
                .Include(v => v.VehicleType)
                .Include(v => v.VehicleBrand)
                .Include(v => v.ServiceBookings)
                .IgnoreQueryFilters()
                .OrderByDescending(v => v.Id)
                .ToListAsync();
        }

        #endregion

        #region Get Single (Details / Edit)

        public async Task<VehicleDetailsViewModel?> GetByIdAsync(int id)
        {
            var entity = await _context.Vehicles
                .Include(v => v.Customer)
                .Include(v => v.VehicleType)
                .Include(v => v.VehicleBrand)
                .Include(v => v.ServiceBookings)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted && v.IsActive);

            return entity == null ? null : _mapper.Map<VehicleDetailsViewModel>(entity);
        }

        public async Task<VehicleFormViewModel?> GetForEditAsync(int id)
        {
            var entity = await _context.Vehicles
                .Include(v => v.Customer)
                .Include(v => v.VehicleType)
                .Include(v => v.VehicleBrand)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted && v.IsActive);

            if (entity == null)
                return null;

            var model = _mapper.Map<VehicleFormViewModel>(entity);
            await BuildFormAsync(model);
            return model;
        }

        public async Task<Vehicle?> GetByIdIncludingDeletedAsync(int id)
        {
            return await _context.Vehicles
                .IgnoreQueryFilters()
                .Include(v => v.Customer)
                .Include(v => v.VehicleType)
                .Include(v => v.VehicleBrand)
                .Include(v => v.ServiceBookings)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        #endregion

        #region Build Form (dropdowns)

        public async Task<VehicleFormViewModel> BuildFormAsync()
        {
            var model = new VehicleFormViewModel();
            await BuildFormAsync(model);
            return model;
        }

        public async Task<VehicleFormViewModel> BuildFormAsync(VehicleFormViewModel model)
        {
            model.Customers = await _context.Customers
                .Where(c => !c.IsDeleted && c.IsActive)
                .OrderBy(c => c.FullName)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FullName} ({c.PhoneNumber})"
                })
                .ToListAsync();

            model.VehicleTypes = await _context.VehicleTypes
                .Where(vt => !vt.IsDeleted && vt.IsActive)
                .OrderBy(vt => vt.TypeName)
                .Select(vt => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = vt.Id.ToString(),
                    Text = vt.TypeName
                })
                .ToListAsync();

            model.VehicleBrands = await _context.VehicleBrands
                .Where(vb => !vb.IsDeleted && vb.IsActive)
                .OrderBy(vb => vb.BrandName)
                .Select(vb => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = vb.Id.ToString(),
                    Text = vb.BrandName
                })
                .ToListAsync();

            return model;
        }

        #endregion

        #region Create

        public async Task<int> CreateAsync(VehicleFormViewModel model)
        {
            if (!await IsRegistrationNumberUniqueAsync(model.RegistrationNumber))
                throw new InvalidOperationException($"Registration number '{model.RegistrationNumber}' already exists.");

            var entity = _mapper.Map<Vehicle>(model);
            entity.IsDeleted = false;
            entity.IsActive = true;
            entity.CreatedOn = DateTime.UtcNow;

            _context.Vehicles.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        #endregion

        #region Update

        public async Task UpdateAsync(VehicleFormViewModel model)
        {
            var entity = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == model.Id && !v.IsDeleted && v.IsActive)
                ?? throw new KeyNotFoundException("Vehicle not found.");

            if (!await IsRegistrationNumberUniqueAsync(model.RegistrationNumber, model.Id))
                throw new InvalidOperationException($"Registration number '{model.RegistrationNumber}' already exists.");

            _mapper.Map(model, entity);
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Validation Helpers

        public async Task<bool> IsRegistrationNumberUniqueAsync(string registrationNumber, int? excludeId = null)
        {
            var query = _context.Vehicles
                .Where(v => v.RegistrationNumber.ToLower() == registrationNumber.ToLower() && !v.IsDeleted && v.IsActive);

            if (excludeId.HasValue)
                query = query.Where(v => v.Id != excludeId.Value);

            return !await query.AnyAsync();
        }

        #endregion

        #region Soft Delete / Activate / Deactivate

        public async Task<bool> DeactivateAsync(int id)
        {
            var vehicle = await _context.Vehicles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted && v.IsActive);

            if (vehicle == null)
                return false;

            if (await _context.ServiceBookings.AnyAsync(sb => sb.VehicleId == id && !sb.IsDeleted && sb.Status != BookingStatus.Completed))
                throw new InvalidOperationException("Cannot deactivate a vehicle with active bookings.");

            vehicle.IsDeleted = true;
            vehicle.IsActive = false;
            vehicle.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivateAsync(int id)
        {
            var vehicle = await _context.Vehicles
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.Id == id && v.IsDeleted && !v.IsActive);

            if (vehicle == null)
                return false;

            vehicle.IsDeleted = false;
            vehicle.IsActive = true;
            vehicle.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SoftDeleteAsync(int id) => await DeactivateAsync(id);
        public async Task<bool> RestoreAsync(int id) => await ActivateAsync(id);

        #endregion

        #region Deleted Vehicles

        public async Task<List<VehicleListViewModel>> GetDeletedAsync()
        {
            var entities = await _context.Vehicles
                .IgnoreQueryFilters()
                .Include(v => v.Customer)
                .Include(v => v.VehicleType)
                .Include(v => v.VehicleBrand)
                .Where(v => v.IsDeleted && !v.IsActive)
                .OrderBy(v => v.RegistrationNumber)
                .ToListAsync();

            return _mapper.Map<List<VehicleListViewModel>>(entities);
        }

        #endregion

        #region Statistics

        public async Task<VehicleStatisticsViewModel> GetStatisticsAsync()
        {
            var allVehicles = await _context.Vehicles
                .IgnoreQueryFilters()
                .Include(v => v.Customer)
                .Include(v => v.VehicleType)
                .Include(v => v.VehicleBrand)
                .Include(v => v.ServiceBookings)
                .ToListAsync();

            var totalRevenue = allVehicles
                .SelectMany(v => v.ServiceBookings)
                .Where(sb => !sb.IsDeleted && sb.Status == BookingStatus.Completed)
                .Sum(sb => sb.TotalAmount);

            var stats = new VehicleStatisticsViewModel
            {
                TotalVehicles = allVehicles.Count,
                ActiveVehicles = allVehicles.Count(v => !v.IsDeleted && v.IsActive),
                DeactiveVehicles = allVehicles.Count(v => v.IsDeleted && !v.IsActive),
                TotalCustomers = allVehicles.Select(v => v.CustomerId).Distinct().Count(),
                TotalServiceRevenue = totalRevenue,
                VehicleTypeDistribution = allVehicles
                    .Where(v => v.VehicleType != null)
                    .GroupBy(v => v.VehicleType.TypeName)
                    .ToDictionary(g => g.Key, g => g.Count()),
                VehicleBrandDistribution = allVehicles
                    .Where(v => v.VehicleBrand != null)
                    .GroupBy(v => v.VehicleBrand.BrandName)
                    .ToDictionary(g => g.Key, g => g.Count()),
                FuelTypeDistribution = allVehicles
                    .GroupBy(v => v.FuelType.ToString())
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            stats.AverageVehiclesPerCustomer = stats.TotalCustomers > 0
                ? (double)stats.TotalVehicles / stats.TotalCustomers
                : 0;

            return stats;
        }

        #endregion

        #region Additional Queries

        public async Task<List<VehicleSummaryViewModel>> GetVehiclesByCustomerAsync(int customerId)
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.VehicleType)
                .Include(v => v.VehicleBrand)
                .Include(v => v.ServiceBookings)
                .Where(v => v.CustomerId == customerId && !v.IsDeleted && v.IsActive)
                .OrderBy(v => v.RegistrationNumber)
                .ToListAsync();

            return _mapper.Map<List<VehicleSummaryViewModel>>(vehicles);
        }

        #endregion
    }
}