using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Application.ViewModels.Vehicle;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Controllers
{
    [Authorize]
    public class VehicleController : Controller
    {
        private readonly VehicleService _service;
        private readonly IMapper _mapper;

        public VehicleController(VehicleService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        // ─── INDEX – with filter, search, date range, and pagination ───
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string filter = "all",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1)
        {
            bool? includeDeleted = filter switch
            {
                "active" => false,
                "deleted" => true,
                _ => null
            };

            var result = await _service.GetPagedAsync(
                search: search,
                includeDeleted: includeDeleted,
                fromDate: fromDate,
                toDate: toDate,
                page: page,
                pageSize: 10
            );

            // Get overall counts (global – ignoring date filters)
            var stats = await _service.GetStatisticsAsync();

            ViewBag.Search = search;
            ViewBag.Filter = filter;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.TotalCount = stats.TotalVehicles;
            ViewBag.ActiveCount = stats.ActiveVehicles;
            ViewBag.DeactiveCount = stats.DeactiveVehicles;

            return View(result);
        }

        // ─── CREATE (GET) ──────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await _service.BuildFormAsync();
            return View(model);
        }

        // ─── CREATE (POST) ─────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await _service.BuildFormAsync(model);
                return View(model);
            }

            try
            {
                await _service.CreateAsync(model);
                TempData["Success"] = "Vehicle created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                model = await _service.BuildFormAsync(model);
                return View(model);
            }
        }

        // ─── EDIT (GET) ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _service.GetForEditAsync(id);
            if (model == null)
                return NotFound();

            var form = await _service.BuildFormAsync();
            model.Customers = form.Customers;
            model.VehicleTypes = form.VehicleTypes;
            model.VehicleBrands = form.VehicleBrands;

            return View(model);
        }

        // ─── EDIT (POST) ───────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VehicleFormViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                var form = await _service.BuildFormAsync();
                model.Customers = form.Customers;
                model.VehicleTypes = form.VehicleTypes;
                model.VehicleBrands = form.VehicleBrands;
                return View(model);
            }

            try
            {
                await _service.UpdateAsync(model);
                TempData["Success"] = "Vehicle updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                var form = await _service.BuildFormAsync();
                model.Customers = form.Customers;
                model.VehicleTypes = form.VehicleTypes;
                model.VehicleBrands = form.VehicleBrands;
                return View(model);
            }
        }

        // ─── DETAILS ──────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            // Get the vehicle even if it's deactivated
            var vehicle = await _service.GetByIdIncludingDeletedAsync(id);
            if (vehicle == null)
                return NotFound();

            var model = _mapper.Map<VehicleDetailsViewModel>(vehicle);
            return View(model);
        }

        // ─── DEACTIVATE (Soft Delete) ──────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                bool result = await _service.DeactivateAsync(id);
                TempData[result ? "Success" : "Error"] = result
                    ? "Vehicle deactivated successfully."
                    : "Vehicle not found or already deactivated.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { filter = "all" });
        }

        // ─── ACTIVATE (Restore) ────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                bool result = await _service.ActivateAsync(id);
                TempData[result ? "Success" : "Error"] = result
                    ? "Vehicle activated successfully."
                    : "Vehicle not found or already active.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { filter = "all" });
        }
    }
}