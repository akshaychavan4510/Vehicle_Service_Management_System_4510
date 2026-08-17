using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Application.ViewModels.Payment;
using Vehicle_Service_Management_System.Domain.Enums;

namespace Vehicle_Service_Management_System.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly PaymentService _service;
        private readonly InvoiceService _invoiceService;

        public PaymentController(PaymentService service, InvoiceService invoiceService)
        {
            _service = service;
            _invoiceService = invoiceService;
        }

        // ============================================================
        // LIST
        // ============================================================
        public async Task<IActionResult> Index(
            string? search,
            int? invoiceId,
            PaymentMode? paymentMode,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page = 1)
        {
            var result = await _service.GetPagedAsync(
                search,
                invoiceId,
                paymentMode,
                dateFrom,
                dateTo,
                null,
                null,
                page,
                10);

            ViewBag.Search = search;
            ViewBag.InvoiceId = invoiceId;
            ViewBag.PaymentMode = paymentMode;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            return View(result);
        }

        // ============================================================
        // SELECT INVOICE (for payment)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> SelectInvoice()
        {
            // ✅ Ensure InvoiceService has a method: GetInvoicesWithOutstandingBalanceAsync()
            var invoices = await _invoiceService.GetInvoicesWithOutstandingBalanceAsync();
            return View(invoices);
        }

        // ============================================================
        // CREATE (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Create(int? invoiceId)
        {
            if (!invoiceId.HasValue)
                return RedirectToAction(nameof(SelectInvoice));

            var invoice = await _invoiceService.GetByIdAsync(invoiceId.Value);
            if (invoice is null)
                return NotFound();

            var model = new PaymentCreateViewModel
            {
                InvoiceId = invoiceId.Value,
                PaymentDate = DateTime.Now
            };

            ViewBag.InvoiceNumber = invoice.InvoiceNumber;
            ViewBag.GrandTotal = invoice.GrandTotal;
            ViewBag.Balance = invoice.Balance;

            return View(model);
        }

        // ============================================================
        // CREATE (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var invoice = await _invoiceService.GetByIdAsync(model.InvoiceId);
                if (invoice != null)
                {
                    ViewBag.InvoiceNumber = invoice.InvoiceNumber;
                    ViewBag.GrandTotal = invoice.GrandTotal;
                    ViewBag.Balance = invoice.Balance;
                }
                return View(model);
            }

            try
            {
                await _service.CreateAsync(model);
                TempData["Success"] = "Payment recorded successfully.";
                return RedirectToAction("Details", "Invoice", new { id = model.InvoiceId });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var invoice = await _invoiceService.GetByIdAsync(model.InvoiceId);
                if (invoice != null)
                {
                    ViewBag.InvoiceNumber = invoice.InvoiceNumber;
                    ViewBag.GrandTotal = invoice.GrandTotal;
                    ViewBag.Balance = invoice.Balance;
                }
                return View(model);
            }
        }

        // ============================================================
        // DETAILS
        // ============================================================
        public async Task<IActionResult> Details(int id)
        {
            var model = await _service.GetDetailsAsync(id);
            if (model is null)
                return NotFound();

            return View(model);
        }

        // ============================================================
        // EDIT (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var details = await _service.GetDetailsAsync(id);
            if (details is null)
                return NotFound();

            if (!Enum.TryParse<PaymentMode>(details.PaymentMode, out var paymentMode))
                paymentMode = PaymentMode.Cash;

            var model = new PaymentUpdateViewModel
            {
                Id = details.Id,
                PaymentDate = details.PaymentDate,
                PaymentMode = paymentMode,
                AmountPaid = details.AmountPaid,
                TransactionReference = details.TransactionReference,
                Remarks = details.Remarks
            };

            return View(model);
        }

        // ============================================================
        // EDIT (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentUpdateViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _service.UpdateAsync(model);
                TempData["Success"] = "Payment updated successfully.";
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

        // ============================================================
        // SOFT DELETE
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _service.DeleteAsync(id);
                TempData[deleted ? "Success" : "Error"] = deleted
                    ? "Payment deleted successfully."
                    : "Payment not found.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // DELETED (soft‑deleted payments)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            // ✅ Ensure PaymentService has GetDeletedAsync()
            var model = await _service.GetDeletedAsync();
            return View(model);
        }

        // ============================================================
        // RESTORE
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                var restored = await _service.RestoreAsync(id);
                TempData[restored ? "Success" : "Error"] = restored
                    ? "Payment restored successfully."
                    : "Payment not found.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}