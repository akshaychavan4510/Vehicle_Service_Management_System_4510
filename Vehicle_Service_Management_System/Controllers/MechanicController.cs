using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Application.ViewModels.Mechanic;

namespace Vehicle_Service_Management_System.Controllers
{
    [Authorize]
    public class MechanicController : Controller
    {
        private readonly MechanicService _service;

        public MechanicController(MechanicService service)
        {
            _service = service;
        }

        // =============================================================
        // LIST (with search and availability filter)
        // =============================================================
        [HttpGet]
        public async Task<IActionResult> Index(string? search, bool? isAvailable, int page = 1)
        {
            var result = await _service.GetPagedAsync(
                searchTerm: search,
                specialization: null,
                isAvailable: isAvailable,
                minExperience: null,
                maxExperience: null,
                pageNumber: page,
                pageSize: 10
            );

            // ─── Get statistics ───
            var stats = await _service.GetStatisticsAsync();

            ViewBag.Search = search;
            ViewBag.IsAvailable = isAvailable;
            ViewBag.TotalCount = stats.Total;
            ViewBag.AvailableCount = stats.Available;
            ViewBag.BusyCount = stats.Busy;

            return View(result);
        }

        // =============================================================
        // CREATE (GET)
        // =============================================================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new MechanicFormViewModel());
        }

        // =============================================================
        // CREATE (POST)
        // =============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MechanicFormViewModel model)
        {
            // Validate unique phone number
            if (!await _service.IsPhoneNumberUniqueAsync(model.PhoneNumber))
                ModelState.AddModelError(nameof(model.PhoneNumber), "A mechanic with this phone number already exists.");

            // Validate unique email (if provided)
            if (!string.IsNullOrWhiteSpace(model.Email) && !await _service.IsEmailUniqueAsync(model.Email))
                ModelState.AddModelError(nameof(model.Email), "A mechanic with this email already exists.");

            if (!ModelState.IsValid)
                return View(model);

            await _service.CreateAsync(model);
            TempData["Success"] = "Mechanic created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // =============================================================
        // EDIT (GET)
        // =============================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _service.GetForEditAsync(id);
            if (model is null)
                return NotFound();

            return View(model);
        }

        // =============================================================
        // EDIT (POST)
        // =============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MechanicFormViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            // Validate unique phone number (excluding current)
            if (!await _service.IsPhoneNumberUniqueAsync(model.PhoneNumber, model.Id))
                ModelState.AddModelError(nameof(model.PhoneNumber), "A mechanic with this phone number already exists.");

            // Validate unique email (excluding current)
            if (!string.IsNullOrWhiteSpace(model.Email) && !await _service.IsEmailUniqueAsync(model.Email, model.Id))
                ModelState.AddModelError(nameof(model.Email), "A mechanic with this email already exists.");

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _service.UpdateAsync(model);
                TempData["Success"] = "Mechanic updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // =============================================================
        // DETAILS
        // =============================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _service.GetByIdAsync(id);
            if (model is null)
                return NotFound();

            return View(model);
        }

        // =============================================================
        // DELETED (List of soft‑deleted mechanics)
        // =============================================================
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var mechanics = await _service.GetDeletedAsync();
            return View(mechanics);
        }

        // =============================================================
        // RESTORE (soft‑deleted → active)
        // =============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            bool restored = await _service.RestoreAsync(id);

            TempData[restored ? "Success" : "Error"] = restored
                ? "Mechanic restored successfully."
                : "Mechanic not found.";

            return RedirectToAction(nameof(Deleted));
        }

        // =============================================================
        // TOGGLE AVAILABILITY (Available ↔ Busy)
        // =============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            await _service.ToggleAvailabilityAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // =============================================================
        // SOFT DELETE
        // =============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool deleted = await _service.SoftDeleteAsync(id);

                TempData[deleted ? "Success" : "Error"] = deleted
                    ? "Mechanic deleted successfully."
                    : "Mechanic not found.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}