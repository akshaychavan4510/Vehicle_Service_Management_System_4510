using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Application.ViewModels.SparePart;

namespace Vehicle_Service_Management_System.Controllers
{
    [Authorize]
    public class SparePartController : Controller
    {
        private readonly SparePartService _service;

        public SparePartController(SparePartService service)
        {
            _service = service;
        }

        // ================================
        // INDEX (list with filters)
        // ================================
        public async Task<IActionResult> Index(
     string? search,
     int? categoryId,
     bool? isActive,
     int page = 1)
        {
            var result = await _service.GetPagedAsync(
                search,
                categoryId,
                null,
                null,
                null,
                null,
                isActive,
                page,
                10);

            // ─── Get statistics ───
            var stats = await _service.GetStatisticsAsync();

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.IsActive = isActive;
            ViewBag.TotalCount = stats.TotalParts;
            ViewBag.ActiveCount = stats.ActiveParts;
            ViewBag.InactiveCount = stats.InactiveParts;
            ViewBag.LowStockCount = stats.LowStockItems;
            ViewBag.TotalStockValue = stats.TotalStockValue;

            return View(result);
        }
        // ================================
        // LOW STOCK
        // ================================
        public async Task<IActionResult> LowStock()
        {
            var items = await _service.GetLowStockItemsAsync();
            return View(items);
        }

        // ================================
        // CREATE (GET)
        // ================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await _service.BuildFormAsync();
            return View(model);
        }

        // ================================
        // CREATE (POST)
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SparePartFormViewModel model)
        {
            if (!await _service.IsPartCodeUniqueAsync(model.PartCode))
                ModelState.AddModelError(nameof(model.PartCode), "A spare part with this code already exists.");

            if (!ModelState.IsValid)
            {
                model = await _service.BuildFormAsync(model);
                return View(model);
            }

            await _service.CreateAsync(model);
            TempData["Success"] = "Spare part created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ================================
        // EDIT (GET)
        // ================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _service.GetForEditAsync(id);
            if (model is null) return NotFound();
            return View(model);
        }

        // ================================
        // EDIT (POST)
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SparePartFormViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!await _service.IsPartCodeUniqueAsync(model.PartCode, model.Id))
                ModelState.AddModelError(nameof(model.PartCode), "A spare part with this code already exists.");

            if (!ModelState.IsValid)
            {
                model = await _service.BuildFormAsync(model);
                return View(model);
            }

            try
            {
                await _service.UpdateAsync(model);
                TempData["Success"] = "Spare part updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // ================================
        // DETAILS
        // ================================
        public async Task<IActionResult> Details(int id)
        {
            var model = await _service.GetByIdAsync(id);
            if (model is null) return NotFound();
            return View(model);
        }

        // ================================
        // UPDATE STOCK (GET)
        // ================================
        [HttpGet]
        public async Task<IActionResult> UpdateStock(int id)
        {
            var part = await _service.GetByIdAsync(id);
            if (part is null) return NotFound();

            var model = new SparePartStockUpdateViewModel
            {
                Id = part.Id,
                PartName = part.PartName,
                CurrentStock = part.StockQuantity,
                NewStockQuantity = part.StockQuantity
            };
            return View(model);
        }

        // ================================
        // UPDATE STOCK (POST)
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStock(SparePartStockUpdateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _service.UpdateStockAsync(model);
            TempData["Success"] = "Stock updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ================================
        // SOFT DELETE (POST)
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.SoftDeleteAsync(id);
                TempData[deleted ? "Success" : "Error"] = deleted ? "Spare part deleted." : "Not found.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // ================================
        // TOGGLE ACTIVE STATUS (POST)
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var toggled = await _service.ToggleActiveStatusAsync(id);
            if (toggled)
                TempData["Success"] = "Status toggled successfully.";
            else
                TempData["Error"] = "Spare part not found.";
            return RedirectToAction(nameof(Index));
        }
    }
}