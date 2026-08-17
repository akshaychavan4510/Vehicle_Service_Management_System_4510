using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Application.ViewModels.ServiceType;

namespace Vehicle_Service_Management_System.Controllers
{
    [Authorize]
    public class ServiceTypeController : Controller
    {
        private readonly ServiceTypeService _service;

        public ServiceTypeController(ServiceTypeService service)
        {
            _service = service;
        }

        // =============================
        // GET: ServiceType/Index
        // =============================
        public async Task<IActionResult> Index(
    string? searchTerm = null,
    bool? includeDeleted = null,
    int page = 1)
        {
            var result = await _service.GetPagedAsync(
                searchTerm: searchTerm,
                includeDeleted: includeDeleted,
                pageNumber: page,
                pageSize: 10
            );

            // ─── Get statistics ───
            var stats = await _service.GetStatisticsAsync();

            ViewBag.Search = searchTerm;
            ViewBag.IncludeDeleted = includeDeleted;
            ViewBag.TotalCount = stats.TotalServices;
            ViewBag.ActiveCount = stats.ActiveServices;
            ViewBag.InactiveCount = stats.InactiveServices;

            return View(result);
        }

        // =============================
        // GET: ServiceType/Create
        // =============================
        [HttpGet]
        public IActionResult Create() => View(new ServiceTypeFormViewModel());

        // =============================
        // POST: ServiceType/Create
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceTypeFormViewModel model)
        {
            if (!await _service.IsServiceNameUniqueAsync(model.ServiceName))
                ModelState.AddModelError(nameof(model.ServiceName), "A service type with this name already exists.");

            if (!ModelState.IsValid)
                return View(model);

            await _service.CreateAsync(model);
            TempData["Success"] = "Service type created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =============================
        // GET: ServiceType/Edit/{id}
        // =============================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _service.GetForEditAsync(id);
            if (model is null) return NotFound();
            return View(model);
        }

        // =============================
        // POST: ServiceType/Edit/{id}
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceTypeFormViewModel model)
        {
            if (id != model.Id) return BadRequest();

            if (!await _service.IsServiceNameUniqueAsync(model.ServiceName, model.Id))
                ModelState.AddModelError(nameof(model.ServiceName), "A service type with this name already exists.");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _service.UpdateAsync(model);
                TempData["Success"] = "Service type updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // =============================
        // GET: ServiceType/Details/{id}
        // =============================
        public async Task<IActionResult> Details(int id)
        {
            var model = await _service.GetByIdAsync(id);
            if (model is null) return NotFound();
            return View(model);
        }

        // =============================
        // POST: ServiceType/Delete/{id} (Soft Delete)
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.SoftDeleteAsync(id);
                TempData[deleted ? "Success" : "Error"] = deleted ? "Service type deleted." : "Not found.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // =============================
        // POST: ServiceType/Restore/{id}
        // =============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            bool restored = await _service.RestoreAsync(id);
            TempData[restored ? "Success" : "Error"] = restored ? "Service type restored." : "Not found.";
            return RedirectToAction(nameof(Index));
        }
    }
}