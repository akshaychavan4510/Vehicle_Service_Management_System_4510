using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.ViewModels.Common;
using Vehicle_Service_Management_System.Application.ViewModels.SparePart;
using Vehicle_Service_Management_System.Domain.Entities;
using Vehicle_Service_Management_System.Infrastructure.Data;

namespace Vehicle_Service_Management_System.Application.Services
{
    public class SparePartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public SparePartService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ================================
        // GET ALL (with optional filters)
        // ================================
        public async Task<List<SparePartListViewModel>> GetAllAsync(
            string? search = null,
            bool lowStockOnly = false,
            bool? isActive = null)
        {
            var query = _context.SpareParts
                .Include(s => s.SparePartCategory)
                .Where(s => !s.IsDeleted) // soft-deleted excluded
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s =>
                    s.PartName.Contains(search) ||
                    s.PartCode.Contains(search) ||
                    (s.Brand != null && s.Brand.Contains(search)));
            }

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive.Value);

            var items = await query
                .OrderBy(s => s.PartName)
                .ToListAsync();

            var mapped = _mapper.Map<List<SparePartListViewModel>>(items);

            return lowStockOnly ? mapped.Where(m => m.StockQuantity <= m.MinimumStock).ToList() : mapped;
        }

        // ================================
        // PAGED RESULT (with advanced filters)
        // ================================
        public async Task<PagedResult<SparePartListViewModel>> GetPagedAsync(
            string? searchTerm = null,
            int? categoryId = null,
            string? brand = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? stockStatus = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.SpareParts
                .Include(s => s.SparePartCategory)
                .Where(s => !s.IsDeleted)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(s =>
                    s.PartName.Contains(searchTerm) ||
                    s.PartCode.Contains(searchTerm) ||
                    (s.Brand != null && s.Brand.Contains(searchTerm)));
            }

            if (categoryId.HasValue)
                query = query.Where(s => s.SparePartCategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(brand))
                query = query.Where(s => s.Brand != null && s.Brand.Contains(brand));

            if (minPrice.HasValue)
                query = query.Where(s => s.UnitPrice >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(s => s.UnitPrice <= maxPrice.Value);

            if (stockStatus == "LowStock")
                query = query.Where(s => s.StockQuantity <= s.MinimumStock);
            else if (stockStatus == "InStock")
                query = query.Where(s => s.StockQuantity > s.MinimumStock);

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(s => s.PartName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<SparePartListViewModel>
            {
                Items = _mapper.Map<List<SparePartListViewModel>>(items),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // ================================
        // GET BY ID (details)
        // ================================
        public async Task<SparePartDetailsViewModel?> GetByIdAsync(int id)
        {
            var entity = await _context.SpareParts
                .Include(s => s.SparePartCategory)
                .Include(s => s.InvoiceItems)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            return entity is null ? null : _mapper.Map<SparePartDetailsViewModel>(entity);
        }

        // ================================
        // GET FOR EDIT
        // ================================
        public async Task<SparePartFormViewModel?> GetForEditAsync(int id)
        {
            var entity = await _context.SpareParts
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (entity is null)
                return null;

            var model = _mapper.Map<SparePartFormViewModel>(entity);
            model.Categories = await GetCategorySelectListAsync();
            return model;
        }

        // ================================
        // BUILD FORM (with category dropdown)
        // ================================
        public async Task<SparePartFormViewModel> BuildFormAsync(SparePartFormViewModel? existing = null)
        {
            var model = existing ?? new SparePartFormViewModel();
            model.Categories = await GetCategorySelectListAsync();
            return model;
        }

        // ================================
        // CATEGORY DROPDOWN (only active categories)
        // ================================
        public async Task<List<SelectListItem>> GetCategorySelectListAsync()
        {
            return await _context.SparePartCategories
                .Where(c => !c.IsDeleted && c.IsActive)
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CategoryName
                })
                .ToListAsync();
        }

        // ================================
        // UNIQUENESS CHECK
        // ================================
        public async Task<bool> IsPartCodeUniqueAsync(string partCode, int? excludeId = null)
        {
            var query = _context.SpareParts
                .Where(s => s.PartCode.ToLower() == partCode.ToLower() && !s.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(s => s.Id != excludeId.Value);

            return !await query.AnyAsync();
        }

        // ================================
        // CREATE (from FormViewModel)
        // ================================
        public async Task<int> CreateAsync(SparePartFormViewModel model)
        {
            if (!await IsPartCodeUniqueAsync(model.PartCode))
                throw new InvalidOperationException($"Part code '{model.PartCode}' already exists.");

            var entity = _mapper.Map<SparePart>(model);
            entity.CreatedOn = DateTime.UtcNow;
            entity.IsDeleted = false;
            entity.IsActive = model.IsActive; // use model value (default true)

            _context.SpareParts.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        // ================================
        // CREATE (from CreateViewModel)
        // ================================
        public async Task<int> CreateFromViewModelAsync(SparePartCreateViewModel model)
        {
            if (!await IsPartCodeUniqueAsync(model.PartCode))
                throw new InvalidOperationException($"Part code '{model.PartCode}' already exists.");

            var entity = _mapper.Map<SparePart>(model);
            entity.CreatedOn = DateTime.UtcNow;
            entity.IsDeleted = false;
            entity.IsActive = true; // new parts are active by default

            _context.SpareParts.Add(entity);
            await _context.SaveChangesAsync();
            return entity.Id;
        }

        // ================================
        // UPDATE (from FormViewModel)
        // ================================
        public async Task UpdateAsync(SparePartFormViewModel model)
        {
            var entity = await _context.SpareParts
                .FirstOrDefaultAsync(s => s.Id == model.Id && !s.IsDeleted)
                ?? throw new KeyNotFoundException("Spare part not found.");

            if (!await IsPartCodeUniqueAsync(model.PartCode, model.Id))
                throw new InvalidOperationException($"Part code '{model.PartCode}' already exists.");

            _mapper.Map(model, entity);
            entity.ModifiedOn = DateTime.UtcNow;
            entity.IsActive = model.IsActive; // ensure active status is updated

            await _context.SaveChangesAsync();
        }

        // ================================
        // UPDATE (from UpdateViewModel)
        // ================================
        public async Task UpdateAsync(SparePartUpdateViewModel model)
        {
            var entity = await _context.SpareParts
                .FirstOrDefaultAsync(s => s.Id == model.Id && !s.IsDeleted)
                ?? throw new KeyNotFoundException("Spare part not found.");

            if (!await IsPartCodeUniqueAsync(model.PartCode, model.Id))
                throw new InvalidOperationException($"Part code '{model.PartCode}' already exists.");

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
            var entity = await _context.SpareParts
                .Include(s => s.InvoiceItems)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (entity is null)
                return false;

            // Prevent deletion if used in any invoice item
            if (entity.InvoiceItems.Any())
                throw new InvalidOperationException("Cannot delete a spare part that has been used in invoices.");

            entity.IsDeleted = true;
            entity.IsActive = false; // deactivate as well
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ================================
        // RESTORE (undo soft delete)
        // ================================
        public async Task<bool> RestoreAsync(int id)
        {
            var entity = await _context.SpareParts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == id && s.IsDeleted);

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
            var entity = await _context.SpareParts
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (entity is null)
                return false;

            _context.SpareParts.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        // ================================
        // STOCK DEDUCTION (used in invoices)
        // ================================
        public async Task DeductStockAsync(int sparePartId, int quantity)
        {
            var part = await _context.SpareParts
                .FirstOrDefaultAsync(s => s.Id == sparePartId && !s.IsDeleted)
                ?? throw new KeyNotFoundException("Spare part not found.");

            if (part.StockQuantity < quantity)
                throw new InvalidOperationException(
                    $"Not enough stock for '{part.PartName}'. " +
                    $"Available: {part.StockQuantity}, requested: {quantity}.");

            part.StockQuantity -= quantity;
            part.ModifiedOn = DateTime.UtcNow;

            // Optional: trigger low stock alert
            if (part.StockQuantity <= part.MinimumStock)
            {
                // log or notify
            }

            await _context.SaveChangesAsync();
        }

        // ================================
        // STOCK ADDITION
        // ================================
        public async Task AddStockAsync(int sparePartId, int quantity, string? reason = null)
        {
            var part = await _context.SpareParts
                .FirstOrDefaultAsync(s => s.Id == sparePartId && !s.IsDeleted)
                ?? throw new KeyNotFoundException("Spare part not found.");

            part.StockQuantity += quantity;
            part.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ================================
        // TOGGLE ACTIVE STATUS
        // ================================
        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            var entity = await _context.SpareParts
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

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
        public async Task<SparePartStatisticsViewModel> GetStatisticsAsync()
        {
            var spareParts = await _context.SpareParts
                .Include(s => s.SparePartCategory)
                .Include(s => s.InvoiceItems)
                .Where(s => !s.IsDeleted)
                .ToListAsync();

            var stats = new SparePartStatisticsViewModel
            {
                TotalParts = spareParts.Count,
                ActiveParts = spareParts.Count(s => s.IsActive),
                InactiveParts = spareParts.Count(s => !s.IsActive),
                LowStockItems = spareParts.Count(s => s.StockQuantity <= s.MinimumStock),
                TotalStockValue = spareParts.Sum(s => s.StockQuantity * s.UnitPrice),
                CategoryDistribution = spareParts
                    .Where(s => s.SparePartCategory != null)
                    .GroupBy(s => s.SparePartCategory.CategoryName)
                    .ToDictionary(g => g.Key, g => g.Count()),
                BrandDistribution = spareParts
                    .Where(s => s.Brand != null)
                    .GroupBy(s => s.Brand!)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            if (spareParts.Any())
            {
                stats.AveragePrice = spareParts.Average(s => s.UnitPrice);

                var mostExpensive = spareParts.OrderByDescending(s => s.UnitPrice).FirstOrDefault();
                stats.MostExpensivePart = mostExpensive?.PartName;

                var mostStocked = spareParts.OrderByDescending(s => s.StockQuantity).FirstOrDefault();
                stats.MostStockedPart = mostStocked?.PartName;

                var mostUsed = spareParts.OrderByDescending(s => s.InvoiceItems.Count).FirstOrDefault();
                stats.MostUsedPart = mostUsed?.PartName;
            }

            return stats;
        }

        // ================================
        // LOW STOCK ITEMS
        // ================================
        public async Task<List<SparePartLowStockViewModel>> GetLowStockItemsAsync()
        {
            var items = await _context.SpareParts
                .Include(s => s.SparePartCategory)
                .Where(s => !s.IsDeleted && s.IsActive && s.StockQuantity <= s.MinimumStock)
                .OrderBy(s => s.StockQuantity)
                .ToListAsync();

            return _mapper.Map<List<SparePartLowStockViewModel>>(items);
        }

        // ================================
        // DROPDOWN (for use in forms)
        // ================================
        public async Task<Dictionary<int, string>> GetSparePartDropdownAsync()
        {
            return await _context.SpareParts
                .Where(s => !s.IsDeleted && s.IsActive && s.StockQuantity > 0)
                .OrderBy(s => s.PartName)
                .ToDictionaryAsync(s => s.Id, s => $"{s.PartName} ({s.StockQuantity} in stock)");
        }

        // ================================
        // EXISTENCE & STOCK CHECKS
        // ================================
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.SpareParts
                .AnyAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<bool> HasSufficientStockAsync(int sparePartId, int quantity)
        {
            var part = await _context.SpareParts
                .FirstOrDefaultAsync(s => s.Id == sparePartId && !s.IsDeleted);

            return part != null && part.StockQuantity >= quantity;
        }

        public async Task<int> GetCurrentStockAsync(int sparePartId)
        {
            var part = await _context.SpareParts
                .FirstOrDefaultAsync(s => s.Id == sparePartId && !s.IsDeleted);

            return part?.StockQuantity ?? 0;
        }

        public async Task<decimal> GetUnitPriceAsync(int sparePartId)
        {
            var part = await _context.SpareParts
                .FirstOrDefaultAsync(s => s.Id == sparePartId && !s.IsDeleted);

            return part?.UnitPrice ?? 0;
        }

        // ================================
        // UPDATE STOCK (manual adjustment)
        // ================================
        public async Task UpdateStockAsync(SparePartStockUpdateViewModel model)
        {
            var entity = await _context.SpareParts
                .FirstOrDefaultAsync(s => s.Id == model.Id && !s.IsDeleted)
                ?? throw new KeyNotFoundException("Spare part not found.");

            entity.StockQuantity = model.NewStockQuantity;
            entity.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ================================
        // GET PARTS BY CATEGORY
        // ================================
        public async Task<List<SparePart>> GetSparePartsByCategoryAsync(int categoryId)
        {
            return await _context.SpareParts
                .Where(s => s.SparePartCategoryId == categoryId && !s.IsDeleted && s.IsActive)
                .OrderBy(s => s.PartName)
                .ToListAsync();
        }
    }
}