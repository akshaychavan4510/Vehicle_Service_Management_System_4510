using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.ViewModels.Common;
using Vehicle_Service_Management_System.Application.ViewModels.VehicleBrand;
using Vehicle_Service_Management_System.Domain.Entities;
using Vehicle_Service_Management_System.Infrastructure.Data;

namespace Vehicle_Service_Management_System.Application.Services
{
    public class VehicleBrandService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public VehicleBrandService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ─── Paged List (with IgnoreQueryFilters) ───
        public async Task<PagedResult<VehicleBrandListViewModel>> GetPagedAsync(
            string? searchTerm = null,
            bool? includeDeleted = null,  // null = all, false = active only, true = deleted only
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.VehicleBrands
                .IgnoreQueryFilters()
                .Include(vb => vb.Vehicles)
                .AsQueryable();

            // ─── Status filter ───
            if (includeDeleted == false)
                query = query.Where(vb => !vb.IsDeleted && vb.IsActive);
            else if (includeDeleted == true)
                query = query.Where(vb => vb.IsDeleted && !vb.IsActive);
            // else: show all (no extra filter)

            // ─── Search ───
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                query = query.Where(vb =>
                    vb.BrandName.Contains(searchTerm) ||
                    (vb.Country != null && vb.Country.Contains(searchTerm)) ||
                    (vb.Description != null && vb.Description.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(vb => vb.BrandName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<VehicleBrandListViewModel>
            {
                Items = _mapper.Map<List<VehicleBrandListViewModel>>(items),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // ─── Non‑paged (for optional use) ───
        public async Task<List<VehicleBrand>> GetAllActiveAsync()
        {
            return await _context.VehicleBrands
                .AsNoTracking()
                .Include(vb => vb.Vehicles)
                .Where(vb => !vb.IsDeleted && vb.IsActive)
                .OrderBy(vb => vb.BrandName)
                .ToListAsync();
        }

        public async Task<List<VehicleBrand>> GetAllDeactivatedAsync()
        {
            return await _context.VehicleBrands
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(vb => vb.Vehicles)
                .Where(vb => vb.IsDeleted && !vb.IsActive)
                .OrderBy(vb => vb.BrandName)
                .ToListAsync();
        }

        public async Task<List<VehicleBrand>> GetAllIncludingDeletedAsync()
        {
            return await _context.VehicleBrands
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(vb => vb.Vehicles)
                .OrderBy(vb => vb.BrandName)
                .ToListAsync();
        }

        // ─── Get Single ───
        public async Task<VehicleBrandDetailsViewModel?> GetByIdAsync(int id)
        {
            var entity = await _context.VehicleBrands
                .Include(vb => vb.Vehicles)
                .FirstOrDefaultAsync(vb => vb.Id == id && !vb.IsDeleted && vb.IsActive);

            return entity == null ? null : _mapper.Map<VehicleBrandDetailsViewModel>(entity);
        }

        public async Task<VehicleBrandFormViewModel?> GetForEditAsync(int id)
        {
            var entity = await _context.VehicleBrands
                .FirstOrDefaultAsync(vb => vb.Id == id && !vb.IsDeleted && vb.IsActive);

            if (entity == null)
                return null;

            return _mapper.Map<VehicleBrandFormViewModel>(entity);
        }

        public async Task<VehicleBrand?> GetByIdIncludingDeletedAsync(int id)
        {
            return await _context.VehicleBrands
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(vb => vb.Id == id);
        }

        // ─── Create ───
        public async Task<int> CreateAsync(VehicleBrandFormViewModel model)
        {
            if (!await IsBrandNameUniqueAsync(model.BrandName))
                throw new InvalidOperationException($"Vehicle brand '{model.BrandName}' already exists.");

            var entity = _mapper.Map<VehicleBrand>(model);

            // ✅ Use the value from the view model
            entity.IsDeleted = model.IsDeleted;
            entity.IsActive = !model.IsDeleted;   // active = not deleted
            entity.CreatedOn = DateTime.UtcNow;

            _context.VehicleBrands.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        public async Task<int> CreateFromViewModelAsync(VehicleBrandCreateViewModel model)
        {
            if (!await IsBrandNameUniqueAsync(model.BrandName))
                throw new InvalidOperationException($"Vehicle brand '{model.BrandName}' already exists.");

            var entity = _mapper.Map<VehicleBrand>(model);
            entity.IsDeleted = false;   // create view model doesn't have IsDeleted, default active
            entity.IsActive = true;
            entity.CreatedOn = DateTime.UtcNow;

            _context.VehicleBrands.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        // ─── Update ───
        public async Task UpdateAsync(VehicleBrandFormViewModel model)
        {
            var entity = await _context.VehicleBrands
                .FirstOrDefaultAsync(vb => vb.Id == model.Id && !vb.IsDeleted && vb.IsActive)
                ?? throw new KeyNotFoundException("Vehicle brand not found.");

            if (!await IsBrandNameUniqueAsync(model.BrandName, model.Id))
                throw new InvalidOperationException($"Vehicle brand '{model.BrandName}' already exists.");

            _mapper.Map(model, entity);

            // ✅ Update the deletion status from the form
            entity.IsDeleted = model.IsDeleted;
            entity.IsActive = !model.IsDeleted;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(VehicleBrandUpdateViewModel model)
        {
            var entity = await _context.VehicleBrands
                .FirstOrDefaultAsync(vb => vb.Id == model.Id && !vb.IsDeleted && vb.IsActive)
                ?? throw new KeyNotFoundException("Vehicle brand not found.");

            if (!await IsBrandNameUniqueAsync(model.BrandName, model.Id))
                throw new InvalidOperationException($"Vehicle brand '{model.BrandName}' already exists.");

            _mapper.Map(model, entity);
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ─── Validation ───
        public async Task<bool> IsBrandNameUniqueAsync(string brandName, int? excludeId = null)
        {
            var query = _context.VehicleBrands
                .Where(vb => vb.BrandName.ToLower() == brandName.ToLower() && !vb.IsDeleted && vb.IsActive);

            if (excludeId.HasValue)
                query = query.Where(vb => vb.Id != excludeId.Value);

            return !await query.AnyAsync();
        }

        // ─── Deactivate (Soft Delete) ───
        public async Task<bool> DeactivateAsync(int id)
        {
            var entity = await _context.VehicleBrands
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(vb => vb.Id == id && !vb.IsDeleted && vb.IsActive);

            if (entity == null)
                return false;

            // Check if any vehicles are still using this brand
            bool hasActiveVehicles = await _context.Vehicles
                .IgnoreQueryFilters()
                .AnyAsync(v => v.VehicleBrandId == id && !v.IsDeleted && v.IsActive);

            if (hasActiveVehicles)
                throw new InvalidOperationException("Cannot deactivate a brand that has active vehicles.");

            entity.IsDeleted = true;
            entity.IsActive = false;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Activate (Restore) ───
        public async Task<bool> ActivateAsync(int id)
        {
            var entity = await _context.VehicleBrands
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(vb => vb.Id == id && vb.IsDeleted && !vb.IsActive);

            if (entity == null)
                return false;

            entity.IsDeleted = false;
            entity.IsActive = true;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Aliases for compatibility ───
        public async Task<bool> SoftDeleteAsync(int id) => await DeactivateAsync(id);
        public async Task<bool> RestoreAsync(int id) => await ActivateAsync(id);

        // ─── Hard Delete (use cautiously) ───
        public async Task<bool> HardDeleteAsync(int id)
        {
            var entity = await _context.VehicleBrands
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(vb => vb.Id == id);

            if (entity == null)
                return false;

            _context.VehicleBrands.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // ─── Statistics ───
        public async Task<VehicleBrandStatisticsViewModel> GetStatisticsAsync()
        {
            var allBrands = await _context.VehicleBrands
                .IgnoreQueryFilters()
                .Include(vb => vb.Vehicles)
                .ToListAsync();

            var stats = new VehicleBrandStatisticsViewModel
            {
                TotalBrands = allBrands.Count,
                ActiveBrands = allBrands.Count(vb => !vb.IsDeleted && vb.IsActive),
                DeactiveBrands = allBrands.Count(vb => vb.IsDeleted && !vb.IsActive),
                TotalVehicles = allBrands.Sum(vb => vb.Vehicles.Count(v => !v.IsDeleted)),
                CountryDistribution = allBrands
                    .Where(vb => vb.Country != null)
                    .GroupBy(vb => vb.Country!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                VehicleDistribution = allBrands
                    .Where(vb => vb.Vehicles.Any(v => !v.IsDeleted))
                    .ToDictionary(vb => vb.BrandName, vb => vb.Vehicles.Count(v => !v.IsDeleted))
            };

            if (allBrands.Any())
            {
                var brandsWithVehicles = allBrands.Where(vb => vb.Vehicles.Any(v => !v.IsDeleted)).ToList();
                stats.AverageVehiclesPerBrand = brandsWithVehicles.Any()
                    ? brandsWithVehicles.Average(vb => vb.Vehicles.Count(v => !v.IsDeleted))
                    : 0;

                var brandWithMostVehicles = allBrands
                    .OrderByDescending(vb => vb.Vehicles.Count(v => !v.IsDeleted))
                    .FirstOrDefault();
                stats.BrandWithMostVehicles = brandWithMostVehicles?.BrandName;
            }

            return stats;
        }

        // ─── Get Active Brands (non‑deleted) ───
        public async Task<List<VehicleBrand>> GetActiveBrandsAsync()
        {
            return await _context.VehicleBrands
                .Where(vb => !vb.IsDeleted && vb.IsActive)
                .OrderBy(vb => vb.BrandName)
                .ToListAsync();
        }

        // ─── Get Dropdown ───
        public async Task<Dictionary<int, string>> GetBrandDropdownAsync()
        {
            return await _context.VehicleBrands
                .Where(vb => !vb.IsDeleted && vb.IsActive)
                .OrderBy(vb => vb.BrandName)
                .ToDictionaryAsync(vb => vb.Id, vb => vb.BrandName);
        }

        // ─── Exists ───
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.VehicleBrands
                .AnyAsync(vb => vb.Id == id && !vb.IsDeleted && vb.IsActive);
        }

        // ─── Get Vehicle Count ───
        public async Task<int> GetVehicleCountForBrandAsync(int brandId)
        {
            return await _context.Vehicles
                .CountAsync(v => v.VehicleBrandId == brandId && !v.IsDeleted && v.IsActive);
        }

        // ─── Get Distinct Countries ───
        public async Task<List<string>> GetDistinctCountriesAsync()
        {
            return await _context.VehicleBrands
                .Where(vb => !vb.IsDeleted && vb.IsActive && vb.Country != null)
                .Select(vb => vb.Country!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
    }
}