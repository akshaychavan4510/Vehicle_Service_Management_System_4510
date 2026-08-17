using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Application.ViewModels.Invoice;

namespace Vehicle_Service_Management_System.Controllers
{
    [Authorize]
    public class InvoiceController : Controller
    {
        private readonly InvoiceService _service;
        private readonly ServiceBookingService _bookingService;

        public InvoiceController(
            InvoiceService service,
            ServiceBookingService bookingService)
        {
            _service = service;
            _bookingService = bookingService;
        }

        // ============================================================
        // INDEX
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                if (page < 1)
                    page = 1;

                if (pageSize < 1)
                    pageSize = 10;

                if (pageSize > 100)
                    pageSize = 100;

                if (dateFrom.HasValue &&
                    dateTo.HasValue &&
                    dateFrom.Value.Date > dateTo.Value.Date)
                {
                    TempData["Error"] =
                        "From date cannot be greater than To date.";

                    dateFrom = null;
                    dateTo = null;
                }

                var result = await _service.GetPaginatedAsync(
                    page,
                    pageSize,
                    search,
                    dateFrom,
                    dateTo);

                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalRecords = result.TotalRecords;
                ViewBag.Search = search;
                ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
                ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

                return View(result.Items);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to load invoices: " + ex.Message;

                ViewBag.CurrentPage = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalRecords = 0;
                ViewBag.Search = search;
                ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
                ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

                return View(new List<InvoiceListViewModel>());
            }
        }

        // ============================================================
        // CREATE - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Create(int? bookingId)
        {
            try
            {
                if (!bookingId.HasValue || bookingId.Value <= 0)
                {
                    var bookings =
                        await _bookingService.GetBookingsWithoutInvoiceAsync();

                    if (bookings == null || !bookings.Any())
                    {
                        TempData["Error"] =
                            "No bookings are available for invoicing.";

                        return RedirectToAction(nameof(Index));
                    }

                    return View("SelectBooking", bookings);
                }

                var model =
                    await _service.BuildCreateFormAsync(bookingId.Value);

                if (model == null)
                {
                    TempData["Error"] =
                        "Unable to prepare the invoice form.";

                    return RedirectToAction(nameof(Index));
                }

                return View("Create", model);
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Error opening invoice creation page: " + ex.Message;

                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // CREATE - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(ValueCountLimit = 4096)]
        public async Task<IActionResult> Create(
            InvoiceCreateViewModel model)
        {
            if (model.BookingId <= 0)
            {
                ModelState.AddModelError(
                    nameof(model.BookingId),
                    "Invalid service booking.");
            }

            if (model.GSTPercentage < 0 ||
                model.GSTPercentage > 100)
            {
                ModelState.AddModelError(
                    nameof(model.GSTPercentage),
                    "GST percentage must be between 0 and 100.");
            }

            if (model.DiscountPercentage < 0 ||
                model.DiscountPercentage > 100)
            {
                ModelState.AddModelError(
                    nameof(model.DiscountPercentage),
                    "Discount percentage must be between 0 and 100.");
            }

            if (model.SparePartsUsed != null)
            {
                var duplicateIds = model.SparePartsUsed
                    .GroupBy(x => x.SparePartId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                foreach (var duplicateId in duplicateIds)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"Spare part ID {duplicateId} was submitted more than once.");
                }

                for (int i = 0;
                     i < model.SparePartsUsed.Count;
                     i++)
                {
                    var item = model.SparePartsUsed[i];

                    if (item == null)
                        continue;

                    if (item.SparePartId <= 0)
                    {
                        ModelState.AddModelError(
                            $"SparePartsUsed[{i}].SparePartId",
                            "Invalid spare part.");
                    }

                    if (item.QuantityUsed <= 0)
                    {
                        ModelState.AddModelError(
                            $"SparePartsUsed[{i}].QuantityUsed",
                            "Quantity must be greater than zero.");
                    }

                    if (item.QuantityUsed > item.AvailableStock)
                    {
                        ModelState.AddModelError(
                            $"SparePartsUsed[{i}].QuantityUsed",
                            $"Cannot exceed available stock ({item.AvailableStock}).");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                await ReloadCreateModelAsync(model);
                return View("Create", model);
            }

            try
            {
                var invoiceId =
                    await _service.CreateAsync(model);

                TempData["Success"] =
                    "Invoice generated successfully.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = invoiceId });
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                await ReloadCreateModelAsync(model);
                return View("Create", model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                await ReloadCreateModelAsync(model);
                return View("Create", model);
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The invoice could not be saved because of a database error.");

                await ReloadCreateModelAsync(model);
                return View("Create", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An error occurred while creating the invoice: " +
                    ex.Message);

                await ReloadCreateModelAsync(model);
                return View("Create", model);
            }
        }

        // ============================================================
        // RELOAD CREATE MODEL
        // ============================================================

        private async Task ReloadCreateModelAsync(
            InvoiceCreateViewModel model)
        {
            try
            {
                if (model.BookingId <= 0)
                    return;

                var refreshed =
                    await _service.BuildCreateFormAsync(model.BookingId);

                if (refreshed == null)
                    return;

                var submittedQuantities =
                    model.SparePartsUsed?
                        .Where(x => x != null && x.QuantityUsed > 0)
                        .GroupBy(x => x.SparePartId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.First().QuantityUsed)
                    ?? new Dictionary<int, int>();

                foreach (var part in refreshed.SparePartsUsed)
                {
                    if (submittedQuantities.TryGetValue(
                        part.SparePartId,
                        out var quantity))
                    {
                        part.QuantityUsed = quantity;
                    }
                }

                model.SparePartsUsed =
                    refreshed.SparePartsUsed;

                model.LabourCharge =
                    refreshed.LabourCharge;

                model.SparePartsTotal =
                    model.SparePartsUsed
                        .Where(x => x.QuantityUsed > 0)
                        .Sum(x => x.UnitPrice * x.QuantityUsed);
            }
            catch
            {
                // Preserve existing ModelState errors.
            }
        }

        // ============================================================
        // DETAILS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
                return NotFound();

            try
            {
                var model =
                    await _service.GetByIdAsync(id);

                if (model == null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to load invoice: " + ex.Message;

                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // PRINT
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Print(int id)
        {
            if (id <= 0)
                return NotFound();

            try
            {
                var model =
                    await _service.GetPrintAsync(id);

                if (model == null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to print invoice: " + ex.Message;

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
        }

        // ============================================================
        // EDIT - GET
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
                return NotFound();

            try
            {
                var model =
                    await _service.GetForEditAsync(id);

                if (model == null)
                    return NotFound();

                return View(model);
            }
            catch (KeyNotFoundException ex)
            {
                TempData["Error"] = ex.Message;

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to edit invoice: " + ex.Message;

                return RedirectToAction(
                    nameof(Details),
                    new { id });
            }
        }

        // ============================================================
        // EDIT - POST
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            InvoiceUpdateViewModel model)
        {
            if (model.Id <= 0)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _service.UpdateAsync(model);

                TempData["Success"] =
                    "Invoice updated successfully.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = model.Id });
            }
            catch (KeyNotFoundException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    ex.Message);

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update invoice: " + ex.Message);

                return View(model);
            }
        }

        // ============================================================
        // MARK AS PAID
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            if (id <= 0)
                return NotFound();

            try
            {
                var success =
                    await _service.MarkAsPaidAsync(id);

                TempData[success ? "Success" : "Error"] =
                    success
                        ? "Invoice marked as paid."
                        : "Invoice not found.";
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to mark invoice as paid: " +
                    ex.Message;
            }

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // ============================================================
        // MARK AS UNPAID
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsUnpaid(int id)
        {
            if (id <= 0)
                return NotFound();

            try
            {
                var success =
                    await _service.MarkAsUnpaidAsync(id);

                TempData[success ? "Success" : "Error"] =
                    success
                        ? "Invoice marked as unpaid."
                        : "Invoice not found.";
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to mark invoice as unpaid: " +
                    ex.Message;
            }

            return RedirectToAction(
                nameof(Details),
                new { id });
        }

        // ============================================================
        // DELETE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return NotFound();

            try
            {
                var deleted =
                    await _service.SoftDeleteAsync(id);

                TempData[deleted ? "Success" : "Error"] =
                    deleted
                        ? "Invoice deleted successfully."
                        : "Invoice not found.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to delete invoice: " +
                    ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // DELETED
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            try
            {
                var model =
                    await _service.GetDeletedAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to load deleted invoices: " +
                    ex.Message;

                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // RESTORE
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            if (id <= 0)
                return NotFound();

            try
            {
                var restored =
                    await _service.RestoreAsync(id);

                TempData[restored ? "Success" : "Error"] =
                    restored
                        ? "Invoice restored successfully."
                        : "Invoice not found.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    "Unable to restore invoice: " +
                    ex.Message;
            }

            return RedirectToAction(nameof(Deleted));
        }
    }
}