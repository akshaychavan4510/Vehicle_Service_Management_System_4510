using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Application.ViewModels.VehicleType;

namespace Vehicle_Service_Management_System.Controllers
{
    [Authorize]
    public class VehicleTypeController : Controller
    {
        private readonly VehicleTypeService _service;
        private readonly ILogger<VehicleTypeController> _logger;

        public VehicleTypeController(VehicleTypeService service, ILogger<VehicleTypeController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // =============================
        // GET: VehicleType/Index
        // =============================
        public async Task<IActionResult> Index(
            [FromQuery] string? search,
            [FromQuery] bool? includeDeleted,
            [FromQuery] int page = 1)
        {
            var result = await _service.GetPagedAsync(
                searchTerm: search,
                includeDeleted: includeDeleted,
                hasVehicles: null,
                pageNumber: page,
                pageSize: 10
            );

            // ─── Get statistics ───
            var stats = await _service.GetStatisticsAsync();

            ViewBag.Search = search;
            ViewBag.IncludeDeleted = includeDeleted;
            ViewBag.TotalCount = stats.TotalTypes;
            ViewBag.ActiveCount = stats.ActiveTypes;
            ViewBag.InactiveCount = stats.InactiveTypes;

            return View(result);
        }

        // =============================
        // GET: VehicleType/Create
        // =============================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new VehicleTypeFormViewModel());
        }

        // =============================
        // POST: VehicleType/Create
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleTypeFormViewModel model)
        {
            if (!await _service.IsTypeNameUniqueAsync(model.TypeName))
                ModelState.AddModelError(nameof(model.TypeName), "A vehicle type with this name already exists.");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _service.CreateAsync(model);
                TempData["Success"] = "Vehicle type created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle type: {TypeName}", model.TypeName);
                ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                return View(model);
            }
        }

        // =============================
        // GET: VehicleType/Edit/{id}
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
        // POST: VehicleType/Edit/{id}
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehicleTypeFormViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!await _service.IsTypeNameUniqueAsync(model.TypeName, model.Id))
                ModelState.AddModelError(nameof(model.TypeName), "A vehicle type with this name already exists.");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _service.UpdateAsync(model);
                TempData["Success"] = "Vehicle type updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating type {Id}", id);
                ModelState.AddModelError("", "The record was modified by another user. Please reload and try again.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle type {Id}", id);
                ModelState.AddModelError("", "An error occurred while updating. Please try again.");
                return View(model);
            }
        }

        // =============================
        // GET: VehicleType/Details/{id}
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
        // POST: VehicleType/Delete/{id} (Soft Delete)
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.SoftDeleteAsync(id);
                TempData[deleted ? "Success" : "Error"] = deleted ? "Vehicle type deleted." : "Not found.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle type {Id}", id);
                TempData["Error"] = "An error occurred while deleting.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =============================
        // POST: VehicleType/Restore/{id}
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                bool restored = await _service.RestoreAsync(id);
                TempData[restored ? "Success" : "Error"] = restored ? "Vehicle type restored." : "Not found.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring vehicle type {Id}", id);
                TempData["Error"] = "An error occurred while restoring.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}