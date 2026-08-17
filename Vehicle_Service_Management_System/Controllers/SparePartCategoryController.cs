using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Application.ViewModels.SparePartCategory;

namespace Vehicle_Service_Management_System.Controllers
{
    [Authorize]
    public class SparePartCategoryController : Controller
    {
        private readonly SparePartCategoryService _service;

        public SparePartCategoryController(SparePartCategoryService service)
        {
            _service = service;
        }

        // ================================
        // INDEX (list with pagination & filters)
        // ================================
        public async Task<IActionResult> Index(string? search, bool? isActive, int page = 1)
        {
            var result = await _service.GetPagedAsync(search, isActive, null, page, 10);

            // ─── Get statistics for the dashboard summary ───
            var stats = await _service.GetStatisticsAsync();

            ViewBag.Search = search;
            ViewBag.IsActive = isActive;
            ViewBag.TotalCount = stats.TotalCategories;
            ViewBag.ActiveCount = stats.ActiveCategories;
            ViewBag.InactiveCount = stats.InactiveCategories;
            ViewBag.TotalSpareParts = stats.TotalSpareParts;
            ViewBag.TotalStockValue = stats.TotalStockValue;

            return View(result);
        }

        // ================================
        // CREATE (GET)
        // ================================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new SparePartCategoryFormViewModel { IsActive = true });
        }

        // ================================
        // CREATE (POST)
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SparePartCategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _service.CreateAsync(model);
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // ================================
        // EDIT (GET)
        // ================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _service.GetForEditAsync(id);
            if (model is null)
                return NotFound();

            return View(model);
        }

        // ================================
        // EDIT (POST)
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SparePartCategoryFormViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _service.UpdateAsync(model);
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // ================================
        // DETAILS
        // ================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _service.GetByIdAsync(id);
            if (model is null)
                return NotFound();

            return View(model);
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
                TempData[deleted ? "Success" : "Error"] =
                    deleted ? "Category deleted." : "Category not found.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // ================================
        // RESTORE (undelete)
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var restored = await _service.RestoreAsync(id);
            TempData[restored ? "Success" : "Error"] =
                restored ? "Category restored." : "Category not found or not deleted.";
            return RedirectToAction(nameof(Index));
        }

        // ================================
        // TOGGLE ACTIVE STATUS (AJAX friendly)
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var toggled = await _service.ToggleActiveStatusAsync(id);
            if (toggled)
            {
                TempData["Success"] = "Status toggled successfully.";
            }
            else
            {
                TempData["Error"] = "Category not found.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}