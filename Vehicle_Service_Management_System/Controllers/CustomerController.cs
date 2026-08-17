using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Application.ViewModels.Customer;
using Vehicle_Service_Management_System.Domain.Entities;

namespace Vehicle_Service_Management_System.Web.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly CustomerService _service;
        private readonly IMapper _mapper;

        public CustomerController(CustomerService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        // ─── INDEX – with pagination, filter, search, and date range ───
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string filter = "all",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 10)
        {
            if (page < 1) page = 1;

            // Get paginated data with date filters
            var (items, totalRecords) = await _service.GetPaginatedAsync(
                filter, search, fromDate, toDate, page, pageSize);

            // Get overall counts (unfiltered by date – global counts)
            var (total, active, deleted) = await _service.GetCountsAsync();

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalRecords = totalRecords;    // total matching filter + date range
            ViewBag.Search = search;
            ViewBag.Filter = filter;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.ActiveCount = active;           // overall active count
            ViewBag.DeletedCount = deleted;         // overall deleted count
            ViewBag.TotalCount = total;             // overall total count

            return View(items);
        }

        // ─── CREATE (GET) ───
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CustomerFormViewModel());
        }

        // ─── CREATE (POST) ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _service.CreateAsync(model);
                TempData["Success"] = "Customer created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // ─── EDIT (GET) ───
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _service.GetForEditAsync(id);
            if (model == null)
                return NotFound();

            return View(model);
        }

        // ─── EDIT (POST) ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerFormViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _service.UpdateAsync(model);
                TempData["Success"] = "Customer updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        // ─── DETAILS ───
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _service.GetDetailsAsync(id);
            if (model == null)
                return NotFound();

            return View(model);
        }

        // ─── DEACTIVATE (Soft Delete) ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var result = await _service.DeactivateAsync(id);
            TempData[result ? "Success" : "Error"] = result
                ? "Customer deactivated successfully."
                : "Customer not found or already deactivated.";

            return RedirectToAction(nameof(Index), new { filter = "all" });
        }

        // ─── ACTIVATE (Restore) ───
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var result = await _service.ActivateAsync(id);
            TempData[result ? "Success" : "Error"] = result
                ? "Customer activated successfully."
                : "Customer not found or already active.";

            return RedirectToAction(nameof(Index), new { filter = "all" });
        }
    }
}