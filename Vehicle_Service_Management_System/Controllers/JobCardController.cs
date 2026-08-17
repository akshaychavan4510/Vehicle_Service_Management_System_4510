using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Application.ViewModels.JobCard;

namespace Vehicle_Service_Management_System.Controllers
{
    [Authorize]
    public class JobCardController : Controller
    {
        private readonly JobCardService _service;

        public JobCardController(JobCardService service)
        {
            _service = service;
        }

        // ===============================
        // LIST (with pagination)
        // ===============================
        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 10)
        {
            var result = await _service.GetPagedAsync(search, page, pageSize);
            ViewBag.Search = search;
            return View(result);
        }

        // ===============================
        // DETAILS
        // ===============================
        public async Task<IActionResult> Details(int id)
        {
            var model = await _service.GetByIdAsync(id);
            if (model == null)
                return NotFound();

            return View(model);
        }

        // ===============================
        // CREATE (GET)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Create(int? bookingId)
        {
            var model = await _service.BuildCreateFormAsync();

            if (bookingId.HasValue)
            {
                var selected = model.AvailableBookings.FirstOrDefault(b => b.Value == bookingId.Value.ToString());
                if (selected != null)
                    selected.Selected = true;
                model.BookingId = bookingId.Value;
            }

            return View(model);
        }

        // ===============================
        // CREATE (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobCardCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var refreshed = await _service.BuildCreateFormAsync();
                model.AvailableBookings = refreshed.AvailableBookings;
                return View(model);
            }

            try
            {
                var id = await _service.CreateAsync(model);
                TempData["Success"] = "Job Card created successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var refreshed = await _service.BuildCreateFormAsync();
                model.AvailableBookings = refreshed.AvailableBookings;
                return View(model);
            }
        }

        // ===============================
        // EDIT (GET)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var details = await _service.GetByIdAsync(id);
            if (details == null)
                return NotFound();

            var vm = new JobCardUpdateViewModel
            {
                Id = details.Id,
                JobCardNumber = details.JobCardNumber,
                BookingId = details.BookingId,
                InspectionDate = details.InspectionDate,
                Checklist = details.Checklist,
                MechanicNotes = details.MechanicNotes,
                WorkPerformed = details.WorkPerformed,
                EstimatedCost = details.EstimatedCost,
                ActualCost = details.ActualCost,
                Status = details.Status
            };

            vm.AvailableStatuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "Pending", Text = "Pending" },
                new SelectListItem { Value = "InProgress", Text = "In Progress" },
                new SelectListItem { Value = "Completed", Text = "Completed" },
                new SelectListItem { Value = "Cancelled", Text = "Cancelled" }
            };

            return View(vm);
        }

        // ===============================
        // EDIT (POST)
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(JobCardUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableStatuses = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Pending", Text = "Pending" },
                    new SelectListItem { Value = "InProgress", Text = "In Progress" },
                    new SelectListItem { Value = "Completed", Text = "Completed" },
                    new SelectListItem { Value = "Cancelled", Text = "Cancelled" }
                };
                return View(model);
            }

            try
            {
                await _service.UpdateAsync(model);
                TempData["Success"] = "Job Card updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.AvailableStatuses = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Pending", Text = "Pending" },
                    new SelectListItem { Value = "InProgress", Text = "In Progress" },
                    new SelectListItem { Value = "Completed", Text = "Completed" },
                    new SelectListItem { Value = "Cancelled", Text = "Cancelled" }
                };
                return View(model);
            }
        }

        // ===============================
        // UPDATE STATUS
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(JobCardStatusUpdateViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Details), new { id = model.Id });

            try
            {
                await _service.UpdateStatusAsync(model);
                TempData["Success"] = "Status updated successfully.";
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Job Card not found.";
            }
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        // ===============================
        // PRINT
        // ===============================
        public async Task<IActionResult> Print(int id)
        {
            var model = await _service.GetPrintAsync(id);
            if (model == null)
                return NotFound();

            return View(model);
        }

        // ===============================
        // SOFT DELETE
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _service.SoftDeleteAsync(id);
            TempData[deleted ? "Success" : "Error"] =
                deleted ? "Job Card deleted successfully." : "Job Card not found.";
            return RedirectToAction(nameof(Index));
        }

        // ===============================
        // DELETED LIST
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var model = await _service.GetDeletedAsync();
            return View(model);
        }

        // ===============================
        // RESTORE
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            bool restored = await _service.RestoreAsync(id);
            TempData[restored ? "Success" : "Error"] =
                restored ? "Job Card restored successfully." : "Job Card not found.";
            return RedirectToAction(nameof(Deleted));
        }

        // ===============================
        // HARD DELETE
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id)
        {
            bool deleted = await _service.HardDeleteAsync(id);
            TempData[deleted ? "Success" : "Error"] =
                deleted ? "Job Card permanently deleted." : "Job Card not found.";
            return RedirectToAction(nameof(Deleted));
        }
    }
}