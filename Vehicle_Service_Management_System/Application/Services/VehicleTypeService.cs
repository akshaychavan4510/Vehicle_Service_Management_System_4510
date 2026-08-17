using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.ViewModels.Common;
using Vehicle_Service_Management_System.Application.ViewModels.VehicleType;
using Vehicle_Service_Management_System.Domain.Entities;
using Vehicle_Service_Management_System.Infrastructure.Data;

namespace Vehicle_Service_Management_System.Application.Services
{
    public class VehicleTypeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public VehicleTypeService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ─── Get Paged (with optional includeDeleted filter) ───
        public async Task<PagedResult<VehicleTypeListViewModel>> GetPagedAsync(
            string? searchTerm = null,
            bool? includeDeleted = null,  // null = active only, true = include deleted, false = active only
            bool? hasVehicles = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            // ✅ MUST use IgnoreQueryFilters() when we might need to see deleted records
            var query = _context.VehicleTypes
                .IgnoreQueryFilters()           // allow us to see deleted if we want
                .Include(v => v.Vehicles)
                .AsQueryable();

            // Filter by soft‑delete status
            if (includeDeleted == false)        // explicitly active only
                query = query.Where(v => !v.IsDeleted && v.IsActive);
            else if (includeDeleted == true)    // show only deleted (inactive)
                query = query.Where(v => v.IsDeleted && !v.IsActive);
            // else: show all (no filter)

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(v =>
                    v.TypeName.Contains(searchTerm) ||
                    (v.Description != null && v.Description.Contains(searchTerm)));
            }

            if (hasVehicles.HasValue)
            {
                if (hasVehicles.Value)
                    query = query.Where(v => v.Vehicles.Any(ve => !ve.IsDeleted && ve.IsActive));
                else
                    query = query.Where(v => !v.Vehicles.Any(ve => !ve.IsDeleted && ve.IsActive));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(v => v.TypeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<VehicleTypeListViewModel>
            {
                Items = _mapper.Map<List<VehicleTypeListViewModel>>(items),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // ─── Get By Id (only active) ───
        public async Task<VehicleTypeDetailsViewModel?> GetByIdAsync(int id)
        {
            var entity = await _context.VehicleTypes
                .Include(v => v.Vehicles)
                    .ThenInclude(ve => ve.VehicleBrand)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted && v.IsActive);

            return entity == null ? null : _mapper.Map<VehicleTypeDetailsViewModel>(entity);
        }

        // ─── Get For Edit (active only) ───
        public async Task<VehicleTypeFormViewModel?> GetForEditAsync(int id)
        {
            var entity = await _context.VehicleTypes
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted && v.IsActive);
            return entity == null ? null : _mapper.Map<VehicleTypeFormViewModel>(entity);
        }

        // ─── Unique Type Name (checks active records only) ───
        public async Task<bool> IsTypeNameUniqueAsync(string typeName, int? excludeId = null)
        {
            var query = _context.VehicleTypes
                .Where(v => v.TypeName.ToLower() == typeName.ToLower() && !v.IsDeleted && v.IsActive);
            if (excludeId.HasValue)
                query = query.Where(v => v.Id != excludeId.Value);
            return !await query.AnyAsync();
        }

        // ─── Create (from FormViewModel) ───
        public async Task<int> CreateAsync(VehicleTypeFormViewModel model)
        {
            if (!await IsTypeNameUniqueAsync(model.TypeName))
                throw new InvalidOperationException($"Vehicle type '{model.TypeName}' already exists.");

            var entity = _mapper.Map<VehicleType>(model);
            // ✅ Use the form's IsDeleted flag and sync IsActive
            entity.IsDeleted = model.IsDeleted;
            entity.IsActive = !model.IsDeleted;
            entity.CreatedOn = DateTime.UtcNow;

            _context.VehicleTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        // ─── Create (from CreateViewModel) ───
        public async Task<int> CreateFromViewModelAsync(VehicleTypeCreateViewModel model)
        {
            if (!await IsTypeNameUniqueAsync(model.TypeName))
                throw new InvalidOperationException($"Vehicle type '{model.TypeName}' already exists.");

            var entity = _mapper.Map<VehicleType>(model);
            // CreateViewModel doesn't have IsDeleted, so default to active
            entity.IsDeleted = false;
            entity.IsActive = true;
            entity.CreatedOn = DateTime.UtcNow;

            _context.VehicleTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        // ─── Update (from FormViewModel) ───
        public async Task UpdateAsync(VehicleTypeFormViewModel model)
        {
            var entity = await _context.VehicleTypes
                .FirstOrDefaultAsync(v => v.Id == model.Id && !v.IsDeleted && v.IsActive)
                ?? throw new KeyNotFoundException("Vehicle type not found.");

            if (!await IsTypeNameUniqueAsync(model.TypeName, model.Id))
                throw new InvalidOperationException($"Vehicle type '{model.TypeName}' already exists.");

            _mapper.Map(model, entity);
            // ✅ Apply the deletion status from the form
            entity.IsDeleted = model.IsDeleted;
            entity.IsActive = !model.IsDeleted;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ─── Update (from UpdateViewModel) ───
        public async Task UpdateAsync(VehicleTypeUpdateViewModel model)
        {
            var entity = await _context.VehicleTypes
                .FirstOrDefaultAsync(v => v.Id == model.Id && !v.IsDeleted && v.IsActive)
                ?? throw new KeyNotFoundException("Vehicle type not found.");

            if (!await IsTypeNameUniqueAsync(model.TypeName, model.Id))
                throw new InvalidOperationException($"Vehicle type '{model.TypeName}' already exists.");

            _mapper.Map(model, entity);
            // ✅ Apply deletion status
            entity.IsDeleted = model.IsDeleted;
            entity.IsActive = !model.IsDeleted;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ─── Soft Delete ───
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _context.VehicleTypes
                .Include(v => v.Vehicles)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted && v.IsActive);

            if (entity == null)
                return false;

            // Prevent deletion if active vehicles exist
            bool hasActiveVehicles = await _context.Vehicles
                .IgnoreQueryFilters()
                .AnyAsync(v => v.VehicleTypeId == id && !v.IsDeleted && v.IsActive);

            if (hasActiveVehicles)
                throw new InvalidOperationException("Cannot delete a type that has active vehicles.");

            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Restore ───
        public async Task<bool> RestoreAsync(int id)
        {
            var entity = await _context.VehicleTypes
                .IgnoreQueryFilters()          // must include deleted
                .FirstOrDefaultAsync(v => v.Id == id && v.IsDeleted && !v.IsActive);

            if (entity == null)
                return false;

            entity.IsDeleted = false;
            entity.IsActive = true;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Hard Delete (use sparingly) ───
        public async Task<bool> HardDeleteAsync(int id)
        {
            var entity = await _context.VehicleTypes
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.Id == id);

            if (entity == null)
                return false;

            _context.VehicleTypes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Get Statistics ───
        public async Task<VehicleTypeStatisticsViewModel> GetStatisticsAsync()
        {
            // ✅ Must include all types (active and deleted) for accurate stats
            var allTypes = await _context.VehicleTypes
                .IgnoreQueryFilters()
                .Include(v => v.Vehicles)
                .ToListAsync();

            var activeTypes = allTypes.Where(v => !v.IsDeleted && v.IsActive).ToList();
            var inactiveTypes = allTypes.Where(v => v.IsDeleted && !v.IsActive).ToList();

            var stats = new VehicleTypeStatisticsViewModel
            {
                TotalTypes = allTypes.Count,
                ActiveTypes = activeTypes.Count,
                InactiveTypes = inactiveTypes.Count,
                TotalVehicles = allTypes.Sum(v => v.Vehicles.Count(ve => !ve.IsDeleted && ve.IsActive)),
                VehicleDistribution = activeTypes
                    .Where(v => v.Vehicles.Any(ve => !ve.IsDeleted && ve.IsActive))
                    .ToDictionary(v => v.TypeName, v => v.Vehicles.Count(ve => !ve.IsDeleted && ve.IsActive))
            };

            if (allTypes.Any())
            {
                var typesWithVehicles = activeTypes.Where(v => v.Vehicles.Any(ve => !ve.IsDeleted && ve.IsActive)).ToList();
                stats.AverageVehiclesPerType = typesWithVehicles.Any()
                    ? typesWithVehicles.Average(v => v.Vehicles.Count(ve => !ve.IsDeleted && ve.IsActive))
                    : 0;

                var typeWithMostVehicles = allTypes
                    .OrderByDescending(v => v.Vehicles.Count(ve => !ve.IsDeleted && ve.IsActive))
                    .FirstOrDefault();
                stats.TypeWithMostVehicles = typeWithMostVehicles?.TypeName;
            }

            return stats;
        }

        // ─── Get Active Types (non‑deleted) ───
        public async Task<List<VehicleType>> GetActiveVehicleTypesAsync()
        {
            return await _context.VehicleTypes
                .Where(v => !v.IsDeleted && v.IsActive)
                .OrderBy(v => v.TypeName)
                .ToListAsync();
        }

        // ─── Get Dropdown ───
        public async Task<Dictionary<int, string>> GetTypeDropdownAsync()
        {
            return await _context.VehicleTypes
                .Where(v => !v.IsDeleted && v.IsActive)
                .OrderBy(v => v.TypeName)
                .ToDictionaryAsync(v => v.Id, v => v.TypeName);
        }

        // ─── Exists ───
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.VehicleTypes
                .AnyAsync(v => v.Id == id && !v.IsDeleted && v.IsActive);
        }

        // ─── Get Vehicle Count ───
        public async Task<int> GetVehicleCountForTypeAsync(int typeId)
        {
            return await _context.Vehicles
                .CountAsync(v => v.VehicleTypeId == typeId && !v.IsDeleted && v.IsActive);
        }

        // ─── Has Vehicles ───
        public async Task<bool> HasVehiclesAsync(int id)
        {
            return await _context.Vehicles
                .AnyAsync(v => v.VehicleTypeId == id && !v.IsDeleted && v.IsActive);
        }

        // ─── Get Type Names ───
        public async Task<List<string>> GetTypeNamesAsync()
        {
            return await _context.VehicleTypes
                .Where(v => !v.IsDeleted && v.IsActive)
                .OrderBy(v => v.TypeName)
                .Select(v => v.TypeName)
                .ToListAsync();
        }
    }
}