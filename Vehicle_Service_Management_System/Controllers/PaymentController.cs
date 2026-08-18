using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Application.ViewModels.Common;
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
        // INDEX - List all payments with pagination and filters
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? search = null,
            int? invoiceId = null,
            PaymentMode? paymentMode = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var result = await _service.GetPagedAsync(
                    search,
                    invoiceId,
                    paymentMode,
                    dateFrom,
                    dateTo,
                    null,
                    null,
                    page,
                    pageSize);

                ViewBag.Search = search;
                ViewBag.InvoiceId = invoiceId;
                ViewBag.PaymentMode = paymentMode;
                ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
                ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
                ViewBag.CurrentPage = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalRecords = result.TotalCount;

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load payments: " + ex.Message;
                return View(new PagedResult<PaymentListViewModel>
                {
                    Items = new List<PaymentListViewModel>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = pageSize
                });
            }
        }

        // ============================================================
        // SELECT INVOICE - Show invoices with outstanding balance
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> SelectInvoice()
        {
            try
            {
                var invoices = await _invoiceService.GetInvoicesWithOutstandingBalanceAsync();
                return View(invoices);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load invoices: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // CREATE - GET (Pass Customer & Vehicle to ViewBag)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Create(int? invoiceId)
        {
            try
            {
                if (!invoiceId.HasValue || invoiceId.Value <= 0)
                {
                    TempData["Error"] = "Please select an invoice first.";
                    return RedirectToAction(nameof(SelectInvoice));
                }

                // Get invoice details with customer and vehicle information
                var invoice = await _invoiceService.GetByIdAsync(invoiceId.Value);
                if (invoice is null)
                {
                    TempData["Error"] = "Invoice not found.";
                    return RedirectToAction(nameof(SelectInvoice));
                }

                // Check if invoice is already fully paid
                if (invoice.Balance <= 0)
                {
                    TempData["Error"] = "This invoice is already fully paid. No payment is required.";
                    return RedirectToAction("Details", "Invoice", new { id = invoiceId.Value });
                }

                var model = new PaymentCreateViewModel
                {
                    InvoiceId = invoiceId.Value,
                    PaymentDate = DateTime.Now
                };

                // Pass ALL required data to ViewBag
                ViewBag.InvoiceNumber = invoice.InvoiceNumber;
                ViewBag.GrandTotal = invoice.GrandTotal;
                ViewBag.Balance = invoice.Balance;
                ViewBag.CustomerName = invoice.CustomerName ?? "N/A";
                ViewBag.VehicleNumber = invoice.VehicleRegistrationNumber ?? "N/A";

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load payment form: " + ex.Message;
                return RedirectToAction(nameof(SelectInvoice));
            }
        }

        // ============================================================
        // CREATE - POST (Save payment)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentCreateViewModel model)
        {
            try
            {
                // Validate model
                if (!ModelState.IsValid)
                {
                    await PopulateViewBag(model.InvoiceId);
                    return View(model);
                }

                // Validate amount is positive
                if (model.AmountPaid <= 0)
                {
                    ModelState.AddModelError(nameof(model.AmountPaid), "Amount must be greater than zero.");
                    await PopulateViewBag(model.InvoiceId);
                    return View(model);
                }

                // Validate amount doesn't exceed balance
                var invoice = await _invoiceService.GetByIdAsync(model.InvoiceId);
                if (invoice is null)
                {
                    ModelState.AddModelError(string.Empty, "Invoice not found.");
                    await PopulateViewBag(model.InvoiceId);
                    return View(model);
                }

                if (model.AmountPaid > invoice.Balance)
                {
                    ModelState.AddModelError(nameof(model.AmountPaid),
                        $"Amount cannot exceed the balance due of {invoice.Balance:C}.");
                    await PopulateViewBag(model.InvoiceId);
                    return View(model);
                }

                // Create payment
                await _service.CreateAsync(model);

                TempData["Success"] = $"Payment of {model.AmountPaid:C} recorded successfully.";
                return RedirectToAction("Details", "Invoice", new { id = model.InvoiceId });
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Invoice or payment record not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateViewBag(model.InvoiceId);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Unable to record payment: " + ex.Message);
                await PopulateViewBag(model.InvoiceId);
                return View(model);
            }
        }

        // ============================================================
        // DETAILS - View payment details
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                    return NotFound();

                var model = await _service.GetDetailsAsync(id);
                if (model is null)
                    return NotFound();

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load payment details: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // EDIT - GET (Show edit form)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (id <= 0)
                    return NotFound();

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

                // Pass invoice info to ViewBag (not in model)
                ViewBag.InvoiceNumber = details.InvoiceNumber;
                ViewBag.CustomerName = details.CustomerName;
                ViewBag.VehicleNumber = details.VehicleNumber;

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load payment for editing: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // EDIT - POST (Update payment)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentUpdateViewModel model)
        {
            try
            {
                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                {
                    // Use ViewBag for display data (model doesn't have these properties)
                    ViewBag.InvoiceNumber = ViewBag.InvoiceNumber ?? string.Empty;
                    ViewBag.CustomerName = ViewBag.CustomerName ?? string.Empty;
                    ViewBag.VehicleNumber = ViewBag.VehicleNumber ?? string.Empty;
                    return View(model);
                }

                await _service.UpdateAsync(model);

                TempData["Success"] = "Payment updated successfully.";
                return RedirectToAction(nameof(Details), new { id = model.Id });
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Payment not found.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                // Use ViewBag for display data
                ViewBag.InvoiceNumber = ViewBag.InvoiceNumber ?? string.Empty;
                ViewBag.CustomerName = ViewBag.CustomerName ?? string.Empty;
                ViewBag.VehicleNumber = ViewBag.VehicleNumber ?? string.Empty;
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Unable to update payment: " + ex.Message);
                // Use ViewBag for display data
                ViewBag.InvoiceNumber = ViewBag.InvoiceNumber ?? string.Empty;
                ViewBag.CustomerName = ViewBag.CustomerName ?? string.Empty;
                ViewBag.VehicleNumber = ViewBag.VehicleNumber ?? string.Empty;
                return View(model);
            }
        }

        // ============================================================
        // DELETE - Soft delete payment
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (id <= 0)
                    return NotFound();

                var deleted = await _service.DeleteAsync(id);

                if (deleted)
                {
                    TempData["Success"] = "Payment deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Payment not found.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to delete payment: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // DELETED - View soft-deleted payments
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            try
            {
                var model = await _service.GetDeletedAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to load deleted payments: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ============================================================
        // RESTORE - Restore soft-deleted payment
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            try
            {
                if (id <= 0)
                    return NotFound();

                var restored = await _service.RestoreAsync(id);

                if (restored)
                {
                    TempData["Success"] = "Payment restored successfully.";
                }
                else
                {
                    TempData["Error"] = "Payment not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to restore payment: " + ex.Message;
            }

            return RedirectToAction(nameof(Deleted));
        }

        // ============================================================
        // GET INVOICE SUMMARY - AJAX helper
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetInvoiceSummary(int invoiceId)
        {
            try
            {
                var invoice = await _invoiceService.GetByIdAsync(invoiceId);
                if (invoice is null)
                    return NotFound();

                return Json(new
                {
                    success = true,
                    invoiceNumber = invoice.InvoiceNumber,
                    grandTotal = invoice.GrandTotal,
                    balance = invoice.Balance,
                    customerName = invoice.CustomerName,
                    vehicleNumber = invoice.VehicleRegistrationNumber,
                    email = invoice.Email,
                    phone = invoice.PhoneNumber
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        // ============================================================
        // MARK AS PAID - Mark invoice as paid (convenience method)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkInvoiceAsPaid(int invoiceId)
        {
            try
            {
                var invoice = await _invoiceService.GetByIdAsync(invoiceId);
                if (invoice is null)
                {
                    TempData["Error"] = "Invoice not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (invoice.Balance <= 0)
                {
                    TempData["Error"] = "Invoice is already fully paid.";
                    return RedirectToAction("Details", "Invoice", new { id = invoiceId });
                }

                var model = new PaymentCreateViewModel
                {
                    InvoiceId = invoiceId,
                    PaymentDate = DateTime.Now,
                    PaymentMode = PaymentMode.Cash,
                    AmountPaid = invoice.Balance,
                    Remarks = "Full payment made"
                };

                await _service.CreateAsync(model);

                TempData["Success"] = $"Invoice #{invoice.InvoiceNumber} marked as paid successfully.";
                return RedirectToAction("Details", "Invoice", new { id = invoiceId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to mark invoice as paid: " + ex.Message;
                return RedirectToAction("Details", "Invoice", new { id = invoiceId });
            }
        }

        // ============================================================
        // PRIVATE HELPERS
        // ============================================================

        private async Task PopulateViewBag(int invoiceId)
        {
            try
            {
                var invoice = await _invoiceService.GetByIdAsync(invoiceId);
                if (invoice != null)
                {
                    ViewBag.InvoiceNumber = invoice.InvoiceNumber;
                    ViewBag.GrandTotal = invoice.GrandTotal;
                    ViewBag.Balance = invoice.Balance;
                    ViewBag.CustomerName = invoice.CustomerName ?? "N/A";
                    ViewBag.VehicleNumber = invoice.VehicleRegistrationNumber ?? "N/A";
                }
            }
            catch
            {
                // Ignore errors when populating ViewBag
            }
        }
    }
}