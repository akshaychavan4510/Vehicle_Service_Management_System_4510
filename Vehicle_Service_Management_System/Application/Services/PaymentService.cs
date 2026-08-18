using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.ViewModels.Common;
using Vehicle_Service_Management_System.Application.ViewModels.Payment;
using Vehicle_Service_Management_System.Domain.Entities;
using Vehicle_Service_Management_System.Domain.Enums;
using Vehicle_Service_Management_System.Infrastructure.Data;

namespace Vehicle_Service_Management_System.Application.Services
{
    public class PaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PaymentService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ─── CREATE ───
        public async Task<int> CreateAsync(PaymentCreateViewModel model)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(i => i.Id == model.InvoiceId && !i.IsDeleted)
                ?? throw new KeyNotFoundException("Invoice not found.");

            var alreadyPaid = invoice.Payments.Sum(p => p.AmountPaid);
            var balance = invoice.GrandTotal - alreadyPaid;

            if (model.AmountPaid > balance)
                throw new InvalidOperationException($"Payment amount exceeds the outstanding balance of {balance:C}.");

            // ✅ Fix: Handle nullable PaymentMode
            if (!model.PaymentMode.HasValue)
                throw new InvalidOperationException("Payment mode is required.");

            var payment = new Payment
            {
                InvoiceId = model.InvoiceId,
                PaymentDate = model.PaymentDate != default ? model.PaymentDate : DateTime.UtcNow,
                PaymentMode = model.PaymentMode.Value,  // ✅ Use .Value to convert PaymentMode? to PaymentMode
                AmountPaid = model.AmountPaid,
                TransactionReference = model.TransactionReference,
                Remarks = model.Remarks,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Update invoice paid status
            await UpdateInvoicePaidStatusAsync(invoice.Id);

            return payment.Id;
        }

        // ─── UPDATE ───
        public async Task<bool> UpdateAsync(PaymentUpdateViewModel model)
        {
            var payment = await _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Payments)
                .FirstOrDefaultAsync(p => p.Id == model.Id && !p.IsDeleted)
                ?? throw new KeyNotFoundException("Payment not found.");

            var totalPaidExcludingThis = payment.Invoice.Payments
                .Where(p => p.Id != model.Id && !p.IsDeleted)
                .Sum(p => p.AmountPaid);

            var newTotalPaid = totalPaidExcludingThis + model.AmountPaid;

            if (newTotalPaid > payment.Invoice.GrandTotal)
                throw new InvalidOperationException(
                    $"Payment amount would exceed the invoice grand total of {payment.Invoice.GrandTotal:C}.");

            _mapper.Map(model, payment);
            payment.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update invoice paid status
            await UpdateInvoicePaidStatusAsync(payment.InvoiceId);

            return true;
        }

        // ─── DELETE (Soft) ───
        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (payment is null) return false;

            payment.IsDeleted = true;
            payment.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update invoice paid status
            await UpdateInvoicePaidStatusAsync(payment.InvoiceId);

            return true;
        }

        // ─── RESTORE ───
        public async Task<bool> RestoreAsync(int id)
        {
            var payment = await _context.Payments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted);

            if (payment is null) return false;

            payment.IsDeleted = false;
            payment.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update invoice paid status
            await UpdateInvoicePaidStatusAsync(payment.InvoiceId);

            return true;
        }

        // ─── HARD DELETE ───
        public async Task<bool> HardDeleteAsync(int id)
        {
            var payment = await _context.Payments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment is null) return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();

            // For hard delete, we don't need to update invoice status
            // because the payment is permanently removed.
            return true;
        }

        // ─── GET BY ID (List View) ───
        public async Task<PaymentListViewModel?> GetByIdAsync(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.ServiceBooking)
                        .ThenInclude(sb => sb.Customer)
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.ServiceBooking)
                        .ThenInclude(sb => sb.Vehicle)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            return payment is null ? null : _mapper.Map<PaymentListViewModel>(payment);
        }

        // ─── GET DETAILS ───
        public async Task<PaymentDetailsViewModel?> GetDetailsAsync(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.ServiceBooking)
                        .ThenInclude(sb => sb.Customer)
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.ServiceBooking)
                        .ThenInclude(sb => sb.Vehicle)
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Payments)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (payment is null) return null;

            var viewModel = _mapper.Map<PaymentDetailsViewModel>(payment);
            viewModel.TotalPaid = payment.Invoice.Payments
                .Where(p => !p.IsDeleted)
                .Sum(p => p.AmountPaid);

            return viewModel;
        }

        // ─── GET ALL FOR INVOICE ───
        public async Task<List<PaymentListViewModel>> GetForInvoiceAsync(int invoiceId)
        {
            var payments = await _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.ServiceBooking)
                        .ThenInclude(sb => sb.Customer)
                .Where(p => p.InvoiceId == invoiceId && !p.IsDeleted)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return _mapper.Map<List<PaymentListViewModel>>(payments);
        }

        // ─── PAGED LIST WITH FILTERS ───
        public async Task<PagedResult<PaymentListViewModel>> GetPagedAsync(
            string? searchTerm = null,
            int? invoiceId = null,
            PaymentMode? paymentMode = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var query = _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.ServiceBooking)
                        .ThenInclude(sb => sb.Customer)
                .Where(p => !p.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p =>
                    p.Invoice.InvoiceNumber.Contains(searchTerm) ||
                    (p.TransactionReference != null && p.TransactionReference.Contains(searchTerm)) ||
                    p.Invoice.ServiceBooking.Customer.FullName.Contains(searchTerm) ||
                    p.Invoice.ServiceBooking.Vehicle.RegistrationNumber.Contains(searchTerm));
            }

            if (invoiceId.HasValue)
                query = query.Where(p => p.InvoiceId == invoiceId.Value);

            if (paymentMode.HasValue)
                query = query.Where(p => p.PaymentMode == paymentMode.Value);

            if (dateFrom.HasValue)
                query = query.Where(p => p.PaymentDate >= dateFrom.Value);

            if (dateTo.HasValue)
            {
                var endDate = dateTo.Value.Date.AddDays(1);
                query = query.Where(p => p.PaymentDate < endDate);
            }

            if (minAmount.HasValue)
                query = query.Where(p => p.AmountPaid >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(p => p.AmountPaid <= maxAmount.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(p => p.PaymentDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<PaymentListViewModel>
            {
                Items = _mapper.Map<List<PaymentListViewModel>>(items),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        // ─── HELPERS ───
        public async Task<decimal> GetTotalPaidForInvoiceAsync(int invoiceId)
        {
            return await _context.Payments
                .Where(p => p.InvoiceId == invoiceId && !p.IsDeleted)
                .SumAsync(p => p.AmountPaid);
        }

        public async Task<decimal> GetOutstandingBalanceForInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted)
                ?? throw new KeyNotFoundException("Invoice not found.");

            var totalPaid = await GetTotalPaidForInvoiceAsync(invoiceId);
            return invoice.GrandTotal - totalPaid;
        }

        public async Task<List<PaymentMode>> GetDistinctPaymentModesAsync()
        {
            return await _context.Payments
                .Where(p => !p.IsDeleted)
                .Select(p => p.PaymentMode)
                .Distinct()
                .OrderBy(p => p)
                .ToListAsync();
        }

        public async Task<bool> IsInvoiceFullyPaidAsync(int invoiceId)
        {
            var balance = await GetOutstandingBalanceForInvoiceAsync(invoiceId);
            return balance <= 0;
        }

        public async Task<Dictionary<int, decimal>> GetTotalPaidForMultipleInvoicesAsync(List<int> invoiceIds)
        {
            return await _context.Payments
                .Where(p => invoiceIds.Contains(p.InvoiceId) && !p.IsDeleted)
                .GroupBy(p => p.InvoiceId)
                .Select(g => new { InvoiceId = g.Key, TotalPaid = g.Sum(p => p.AmountPaid) })
                .ToDictionaryAsync(x => x.InvoiceId, x => x.TotalPaid);
        }

        // ─── STATISTICS ───
        public async Task<PaymentStatisticsViewModel> GetStatisticsAsync(
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var query = _context.Payments
                .Where(p => !p.IsDeleted);

            if (dateFrom.HasValue)
                query = query.Where(p => p.PaymentDate >= dateFrom.Value);

            if (dateTo.HasValue)
            {
                var endDate = dateTo.Value.Date.AddDays(1);
                query = query.Where(p => p.PaymentDate < endDate);
            }

            var payments = await query.ToListAsync();

            var stats = new PaymentStatisticsViewModel
            {
                TotalPayments = payments.Count,
                TotalAmount = payments.Sum(p => p.AmountPaid),
                AveragePayment = payments.Any() ? payments.Average(p => p.AmountPaid) : 0,
                TodayPayments = payments.Count(p => p.PaymentDate.Date == DateTime.UtcNow.Date),
                TodayAmount = payments.Where(p => p.PaymentDate.Date == DateTime.UtcNow.Date).Sum(p => p.AmountPaid),
                PaymentModeDistribution = payments
                    .GroupBy(p => p.PaymentMode.ToString())
                    .ToDictionary(g => g.Key, g => g.Sum(p => p.AmountPaid))
            };

            return stats;
        }

        // ─── PRIVATE HELPER: Update Invoice Paid Status ───
        private async Task UpdateInvoicePaidStatusAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

            if (invoice == null) return;

            var totalPaid = invoice.Payments.Where(p => !p.IsDeleted).Sum(p => p.AmountPaid);
            invoice.IsPaid = totalPaid >= invoice.GrandTotal;
            invoice.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ─── GET DELETED ───
        public async Task<List<PaymentListViewModel>> GetDeletedAsync()
        {
            var payments = await _context.Payments
                .IgnoreQueryFilters()
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.ServiceBooking)
                        .ThenInclude(sb => sb.Customer)
                .Where(p => p.IsDeleted)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            return _mapper.Map<List<PaymentListViewModel>>(payments);
        }
    }
}