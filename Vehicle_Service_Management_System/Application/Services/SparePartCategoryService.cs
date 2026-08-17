using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.ViewModels.Common;
using Vehicle_Service_Management_System.Application.ViewModels.SparePartCategory;
using Vehicle_Service_Management_System.Domain.Entities;
using Vehicle_Service_Management_System.Infrastructure.Data;

namespace Vehicle_Service_Management_System.Application.Services
{
    public class SparePartCategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SparePartCategoryService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ================================
        // GET ALL (with optional active filter)
        // ================================
        public async Task<List<SparePartCategoryListViewModel>> GetAllAsync(bool? isActive = null)
        {
            var query = _context.SparePartCategories
                .Include(c => c.SpareParts) // needed for count in mapping
                .Where(c => !c.IsDeleted)
                .AsNoTracking()
                .AsQueryable();

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            var items = await query
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return _mapper.Map<List<SparePartCategoryListViewModel>>(items);
        }

        // ================================
        // PAGED RESULT (with search, filters)
        // ================================
        public async Task<PagedResult<SparePartCategoryListViewModel>> GetPagedAsync(
            string? searchTerm = null,
            bool? isActive = null,
            bool? hasParts = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.SparePartCategories
                .Include(c => c.SpareParts)
                .Where(c => !c.IsDeleted)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c =>
                    c.CategoryName.Contains(searchTerm) ||
                    (c.Description != null && c.Description.Contains(searchTerm)));
            }

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            if (hasParts.HasValue)
            {
                if (hasParts.Value)
                    query = query.Where(c => c.SpareParts.Any(sp => !sp.IsDeleted));
                else
                    query = query.Where(c => !c.SpareParts.Any(sp => !sp.IsDeleted));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.CategoryName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<SparePartCategoryListViewModel>
            {
                Items = _mapper.Map<List<SparePartCategoryListViewModel>>(items),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // ================================
        // GET BY ID (details) – includes only non‑deleted spare parts
        // ================================
        public async Task<SparePartCategoryDetailsViewModel?> GetByIdAsync(int id)
        {
            var entity = await _context.SparePartCategories
                .Include(c => c.SpareParts.Where(sp => !sp.IsDeleted))  // ✅ filter parts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            return entity is null ? null : _mapper.Map<SparePartCategoryDetailsViewModel>(entity);
        }

        // ================================
        // GET FOR EDIT (form view model)
        // ================================
        public async Task<SparePartCategoryFormViewModel?> GetForEditAsync(int id)
        {
            var entity = await _context.SparePartCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            return entity is null ? null : _mapper.Map<SparePartCategoryFormViewModel>(entity);
        }

        // ================================
        // UNIQUENESS CHECK
        // ================================
        public async Task<bool> IsCategoryNameUniqueAsync(string categoryName, int? excludeId = null)
        {
            var query = _context.SparePartCategories
                .Where(c => c.CategoryName.ToLower() == categoryName.ToLower() && !c.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);

            return !await query.AnyAsync();
        }

        // ================================
        // CREATE (using FormViewModel)
        // ================================
        public async Task<int> CreateAsync(SparePartCategoryFormViewModel model)
        {
            if (!await IsCategoryNameUniqueAsync(model.CategoryName))
                throw new InvalidOperationException($"Category '{model.CategoryName}' already exists.");

            var entity = _mapper.Map<SparePartCategory>(model);
            entity.CreatedOn = DateTime.UtcNow;
            entity.IsDeleted = false;
            entity.IsActive = model.IsActive; // default true from view model

            _context.SparePartCategories.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        // ================================
        // CREATE (using CreateViewModel)
        // ================================
        public async Task<int> CreateFromViewModelAsync(SparePartCategoryCreateViewModel model)
        {
            if (!await IsCategoryNameUniqueAsync(model.CategoryName))
                throw new InvalidOperationException($"Category '{model.CategoryName}' already exists.");

            var entity = _mapper.Map<SparePartCategory>(model);
            entity.CreatedOn = DateTime.UtcNow;
            entity.IsDeleted = false;
            entity.IsActive = true; // new categories are active by default

            _context.SparePartCategories.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        // ================================
        // UPDATE (using FormViewModel)
        // ================================
        public async Task UpdateAsync(SparePartCategoryFormViewModel model)
        {
            var entity = await _context.SparePartCategories
                .FirstOrDefaultAsync(c => c.Id == model.Id && !c.IsDeleted)
                ?? throw new KeyNotFoundException("Spare part category not found.");

            if (!await IsCategoryNameUniqueAsync(model.CategoryName, model.Id))
                throw new InvalidOperationException($"Category '{model.CategoryName}' already exists.");

            _mapper.Map(model, entity);
            entity.ModifiedOn = DateTime.UtcNow;
            entity.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
        }

        // ================================
        // UPDATE (using UpdateViewModel)
        // ================================
        public async Task UpdateAsync(SparePartCategoryUpdateViewModel model)
        {
            var entity = await _context.SparePartCategories
                .FirstOrDefaultAsync(c => c.Id == model.Id && !c.IsDeleted)
                ?? throw new KeyNotFoundException("Spare part category not found.");

            if (!await IsCategoryNameUniqueAsync(model.CategoryName, model.Id))
                throw new InvalidOperationException($"Category '{model.CategoryName}' already exists.");

            _mapper.Map(model, entity);
            entity.ModifiedOn = DateTime.UtcNow;
            entity.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
        }

        // ================================
        // SOFT DELETE
        // ================================
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var entity = await _context.SparePartCategories
                .Include(c => c.SpareParts)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (entity is null)
                return false;

            // Prevent deletion if it has any non‑deleted spare parts
            if (entity.SpareParts.Any(sp => !sp.IsDeleted))
                throw new InvalidOperationException("Cannot delete a category that still has spare parts.");

            entity.IsDeleted = true;
            entity.IsActive = false; // optional: deactivate as well
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ================================
        // RESTORE (undelete)
        // ================================
        public async Task<bool> RestoreAsync(int id)
        {
            var entity = await _context.SparePartCategories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDeleted);

            if (entity is null)
                return false;

            entity.IsDeleted = false;
            entity.IsActive = true;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ================================
        // HARD DELETE (permanent)
        // ================================
        public async Task<bool> HardDeleteAsync(int id)
        {
            var entity = await _context.SparePartCategories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (entity is null)
                return false;

            _context.SparePartCategories.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // ================================
        // TOGGLE ACTIVE STATUS
        // ================================
        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            var entity = await _context.SparePartCategories
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (entity is null)
                return false;

            entity.IsActive = !entity.IsActive;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ================================
        // STATISTICS
        // ================================
        public async Task<SparePartCategoryStatisticsViewModel> GetStatisticsAsync()
        {
            var categories = await _context.SparePartCategories
                .Include(c => c.SpareParts)
                .Where(c => !c.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            var stats = new SparePartCategoryStatisticsViewModel
            {
                TotalCategories = categories.Count,
                ActiveCategories = categories.Count(c => c.IsActive),
                InactiveCategories = categories.Count(c => !c.IsActive),
                TotalSpareParts = categories.Sum(c => c.SpareParts.Count(sp => !sp.IsDeleted)),
                TotalStockValue = categories
                    .SelectMany(c => c.SpareParts)
                    .Where(sp => !sp.IsDeleted)
                    .Sum(sp => sp.StockQuantity * sp.UnitPrice),
                PartsPerCategory = categories
                    .Where(c => c.SpareParts.Any(sp => !sp.IsDeleted))
                    .ToDictionary(
                        c => c.CategoryName,
                        c => c.SpareParts.Count(sp => !sp.IsDeleted))
            };

            if (categories.Any())
            {
                var nonEmptyCategories = categories
                    .Where(c => c.SpareParts.Any(sp => !sp.IsDeleted))
                    .ToList();

                if (nonEmptyCategories.Any())
                {
                    stats.AveragePartsPerCategory = nonEmptyCategories
                        .Average(c => c.SpareParts.Count(sp => !sp.IsDeleted));

                    var categoryWithMostParts = nonEmptyCategories
                        .OrderByDescending(c => c.SpareParts.Count(sp => !sp.IsDeleted))
                        .FirstOrDefault();
                    stats.CategoryWithMostParts = categoryWithMostParts?.CategoryName;

                    var categoryWithHighestStockValue = nonEmptyCategories
                        .Select(c => new {
                            c.CategoryName,
                            Value = c.SpareParts.Where(sp => !sp.IsDeleted).Sum(sp => sp.StockQuantity * sp.UnitPrice)
                        })
                        .OrderByDescending(x => x.Value)
                        .FirstOrDefault();
                    stats.CategoryWithHighestStockValue = categoryWithHighestStockValue?.CategoryName;
                }
            }

            return stats;
        }

        // ================================
        // ACTIVE CATEGORIES (for dropdowns)
        // ================================
        public async Task<List<SparePartCategory>> GetActiveCategoriesAsync()
        {
            return await _context.SparePartCategories
                .Where(c => !c.IsDeleted && c.IsActive)
                .OrderBy(c => c.CategoryName)
                .AsNoTracking()
                .ToListAsync();
        }

        // ================================
        // DROPDOWN (Id, Name)
        // ================================
        public async Task<Dictionary<int, string>> GetCategoryDropdownAsync()
        {
            return await _context.SparePartCategories
                .Where(c => !c.IsDeleted && c.IsActive)
                .OrderBy(c => c.CategoryName)
                .AsNoTracking()
                .ToDictionaryAsync(c => c.Id, c => c.CategoryName);
        }

        // ================================
        // EXISTENCE CHECKS
        // ================================
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.SparePartCategories
                .AnyAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<bool> HasSparePartsAsync(int id)
        {
            return await _context.SpareParts
                .AnyAsync(sp => sp.SparePartCategoryId == id && !sp.IsDeleted);
        }

        public async Task<int> GetSparePartCountAsync(int id)
        {
            return await _context.SpareParts
                .CountAsync(sp => sp.SparePartCategoryId == id && !sp.IsDeleted);
        }

        public async Task<decimal> GetCategoryStockValueAsync(int id)
        {
            return await _context.SpareParts
                .Where(sp => sp.SparePartCategoryId == id && !sp.IsDeleted)
                .SumAsync(sp => sp.StockQuantity * sp.UnitPrice);
        }

        public async Task<List<SparePartCategory>> GetCategoriesWithLowStockItemsAsync()
        {
            var categories = await _context.SparePartCategories
                .Include(c => c.SpareParts)
                .Where(c => !c.IsDeleted && c.IsActive)
                .AsNoTracking()
                .ToListAsync();

            return categories
                .Where(c => c.SpareParts.Any(sp => !sp.IsDeleted && sp.StockQuantity <= sp.MinimumStock))
                .ToList();
        }
    }
}