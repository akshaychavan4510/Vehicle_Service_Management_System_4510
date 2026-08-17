using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Application.ViewModels.VehicleBrand;

namespace Vehicle_Service_Management_System.Controllers
{
    [Authorize]
    public class VehicleBrandController : Controller
    {
        private readonly VehicleBrandService _service;
        private readonly ILogger<VehicleBrandController> _logger;

        public VehicleBrandController(VehicleBrandService service, ILogger<VehicleBrandController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // =============================
        // GET: VehicleBrand/Index
        // =============================
        public async Task<IActionResult> Index(
            [FromQuery] bool? IsDeleted,
            [FromQuery] string? search,
            [FromQuery] int page = 1)
        {
            var result = await _service.GetPagedAsync(
                searchTerm: search,
                includeDeleted: IsDeleted,
                pageNumber: page,
                pageSize: 10
            );

            // Get statistics for the stats cards
            var stats = await _service.GetStatisticsAsync();

            ViewBag.IsDeleted = IsDeleted;
            ViewBag.Search = search;
            ViewBag.TotalCount = stats.TotalBrands;
            ViewBag.ActiveCount = stats.ActiveBrands;
            ViewBag.DeletedCount = stats.DeactiveBrands;

            return View(result);
        }

        // =============================
        // GET: VehicleBrand/Create
        // =============================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new VehicleBrandFormViewModel());
        }

        // =============================
        // POST: VehicleBrand/Create
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleBrandFormViewModel model)
        {
            if (!await _service.IsBrandNameUniqueAsync(model.BrandName))
                ModelState.AddModelError(nameof(model.BrandName), "A vehicle brand with this name already exists.");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _service.CreateAsync(model);
                TempData["Success"] = "Vehicle brand created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle brand.");
                ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                return View(model);
            }
        }

        // =============================
        // GET: VehicleBrand/Edit/{id}
        // =============================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _service.GetForEditAsync(id);
            if (model is null)
                return NotFound();
            return View(model);
        }

        // =============================
        // POST: VehicleBrand/Edit/{id}
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehicleBrandFormViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!await _service.IsBrandNameUniqueAsync(model.BrandName, model.Id))
                ModelState.AddModelError(nameof(model.BrandName), "A vehicle brand with this name already exists.");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _service.UpdateAsync(model);
                TempData["Success"] = "Vehicle brand updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating brand {Id}", id);
                ModelState.AddModelError("", "The record was modified by another user. Please reload and try again.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating brand {Id}", id);
                ModelState.AddModelError("", "An error occurred while updating.");
                return View(model);
            }
        }

        // =============================
        // GET: VehicleBrand/Details/{id}
        // =============================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _service.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            return View(model);
        }

        // =============================
        // POST: VehicleBrand/Delete/{id} (Soft Delete)
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.SoftDeleteAsync(id);
                TempData[deleted ? "Success" : "Error"] = deleted ? "Vehicle brand deleted." : "Brand not found.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting brand {Id}", id);
                TempData["Error"] = "An error occurred while deleting.";
            }
            return RedirectToAction(nameof(Index));
        }

        // =============================
        // POST: VehicleBrand/Restore/{id}
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                bool restored = await _service.RestoreAsync(id);
                TempData[restored ? "Success" : "Error"] = restored ? "Vehicle brand restored." : "Brand not found.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring brand {Id}", id);
                TempData["Error"] = "An error occurred while restoring.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}