using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Application.ViewModels.ServiceBooking;
using Vehicle_Service_Management_System.Domain.Enums;
using Vehicle_Service_Management_System.Infrastructure.Data;

namespace Vehicle_Service_Management_System.Controllers
{
    [Authorize]
    public class ServiceBookingController : Controller
    {
        private readonly ServiceBookingService _service;
        private readonly ApplicationDbContext _context;

        public ServiceBookingController(ServiceBookingService service, ApplicationDbContext context)
        {
            _service = service;
            _context = context;
        }

        // ─── INDEX ────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index(
            BookingStatus? status,
            string? search,
            int? customerId,
            int? vehicleId,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page = 1)
        {
            if (page < 1) page = 1;

            var result = await _service.GetPagedAsync(
                status, search, customerId, vehicleId,
                dateFrom, dateTo, page, 10);

            // Get statistics for the header
            var stats = await _service.GetStatisticsAsync(dateFrom, dateTo);

            ViewBag.Status = status;
            ViewBag.Search = search;
            ViewBag.CustomerId = customerId;
            ViewBag.VehicleId = vehicleId;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;

            // Pass stats to view (we'll use them in the header)
            ViewBag.Stats = stats;

            return View("Index", result);
        }

        // ─── CREATE (GET) ────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await _service.BuildFormAsync();
            return View("Create", model);
        }

        // ─── CREATE (POST) ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceBookingCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCreateDropdownsAsync(model);
                return View("Create", model);
            }

            try
            {
                await _service.CreateAsync(model);
                TempData["Success"] = "Booking created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateCreateDropdownsAsync(model);
                return View("Create", model);
            }
        }

        // ─── DETAILS ──────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _service.GetByIdAsync(id);
            if (model is null) return NotFound();
            return View("Details", model);
        }

        // ─── EDIT (GET) ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _service.GetForEditAsync(id);
            if (model is null) return NotFound();

            await PopulateEditDropdownsAsync(model);
            return View("Edit", model);
        }

        // ─── EDIT (POST) ─────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceBookingUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest();

            // Remove dropdown lists from ModelState validation (they are not required)
            ModelState.Remove(nameof(model.Customers));
            ModelState.Remove(nameof(model.Vehicles));
            ModelState.Remove(nameof(model.Mechanics));
            ModelState.Remove(nameof(model.ServiceTypes));
            ModelState.Remove(nameof(model.ServiceTypeOptions));

            if (!ModelState.IsValid)
            {
                await PopulateEditDropdownsAsync(model);
                return View("Edit", model);
            }

            try
            {
                await _service.UpdateAsync(model);
                TempData["Success"] = "Booking updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateEditDropdownsAsync(model);
                return View("Edit", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Unable to update booking: {ex.Message}");
                await PopulateEditDropdownsAsync(model);
                return View("Edit", model);
            }
        }

        // ─── UPDATE STATUS ───────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, BookingStatus status)
        {
            try
            {
                await _service.UpdateStatusAsync(id, status);
                TempData["Success"] = $"Booking status changed to {status}.";
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        // ─── ASSIGN MECHANIC ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMechanic(int id, int mechanicId)
        {
            try
            {
                await _service.AssignMechanicAsync(id, mechanicId);
                TempData["Success"] = "Mechanic assigned successfully.";
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        // ─── CANCEL BOOKING ──────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, string? reason)
        {
            try
            {
                await _service.CancelBookingAsync(id, reason);
                TempData["Success"] = "Booking cancelled.";
            }
            catch (KeyNotFoundException) { return NotFound(); }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Details), new { id });
        }

        // ─── DELETE ───────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteAsync(id);
                if (deleted) TempData["Success"] = "Booking deleted successfully.";
                else TempData["Error"] = "Booking not found.";
            }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
            return RedirectToAction(nameof(Index));
        }

        // ─── AJAX: GET VEHICLES BY CUSTOMER ─────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetVehiclesByCustomer(int customerId)
        {
            var vehicles = await _context.Vehicles
                .Where(v => v.CustomerId == customerId && !v.IsDeleted && v.IsActive)
                .OrderBy(v => v.RegistrationNumber)
                .Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.RegistrationNumber} - {v.VehicleName}"
                })
                .ToListAsync();

            return Ok(vehicles);
        }

        // ─── PRIVATE HELPERS ─────────────────────────────────────────
        private async Task PopulateCreateDropdownsAsync(ServiceBookingCreateViewModel model)
        {
            var refreshed = await _service.BuildFormAsync();
            model.Customers = refreshed.Customers;
            model.Vehicles = refreshed.Vehicles;
            model.Mechanics = refreshed.Mechanics;
            model.ServiceTypes = refreshed.ServiceTypes;
        }

        private async Task PopulateEditDropdownsAsync(ServiceBookingUpdateViewModel model)
        {
            model.Customers = await _context.Customers
                .Where(c => !c.IsDeleted && c.IsActive)
                .OrderBy(c => c.FullName)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.FullName,
                    Selected = c.Id == model.CustomerId
                })
                .ToListAsync();

            model.Vehicles = await _context.Vehicles
                .Where(v => !v.IsDeleted && v.IsActive)
                .OrderBy(v => v.RegistrationNumber)
                .Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.RegistrationNumber} - {v.VehicleName}",
                    Selected = v.Id == model.VehicleId
                })
                .ToListAsync();

            model.Mechanics = await _context.Mechanics
                .Where(m => m.IsAvailable && !m.IsDeleted)
                .OrderBy(m => m.FullName)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.FullName,
                    Selected = m.Id == model.MechanicId
                })
                .ToListAsync();

            model.ServiceTypes = await _context.ServiceTypes
                .Where(st => !st.IsDeleted && st.IsActive)
                .OrderBy(st => st.ServiceName)
                .Select(st => new SelectListItem
                {
                    Value = st.Id.ToString(),
                    Text = $"{st.ServiceName} (Rs. {st.LabourCharge})"
                })
                .ToListAsync();
        }
    }
}