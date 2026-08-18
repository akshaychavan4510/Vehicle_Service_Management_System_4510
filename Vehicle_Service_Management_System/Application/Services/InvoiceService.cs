using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.ViewModels.Invoice;
using Vehicle_Service_Management_System.Domain.Entities;
using Vehicle_Service_Management_System.Domain.Enums;
using Vehicle_Service_Management_System.Infrastructure.Data;

namespace Vehicle_Service_Management_System.Application.Services
{
    public class InvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly SparePartService _sparePartService;

        public InvoiceService(
            ApplicationDbContext context,
            IMapper mapper,
            SparePartService sparePartService)
        {
            _context = context;
            _mapper = mapper;
            _sparePartService = sparePartService;
        }

        // ============================================================
        // BUILD CREATE FORM
        // ============================================================
        public async Task<InvoiceCreateViewModel> BuildCreateFormAsync(int bookingId)
        {
            var booking = await _context.ServiceBookings
                .Include(b => b.Customer)
                .Include(b => b.Vehicle)
                .Include(b => b.ServiceBookingDetails)
                    .ThenInclude(d => d.ServiceType)
                .Include(b => b.Invoice)
                .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

            if (booking == null)
            {
                throw new KeyNotFoundException("Booking not found.");
            }

            if (booking.Status != BookingStatus.Completed)
            {
                throw new InvalidOperationException("An invoice can only be generated for a completed booking.");
            }

            if (booking.Invoice != null && !booking.Invoice.IsDeleted)
            {
                throw new InvalidOperationException("This booking already has an invoice.");
            }

            var labourCharge = booking.ServiceBookingDetails?
                .Where(d => !d.IsDeleted)
                .Sum(d => d.Price * d.Quantity) ?? 0m;

            var spareParts = await _context.SpareParts
                .Where(s => s.StockQuantity > 0 && !s.IsDeleted && s.IsActive)
                .OrderBy(s => s.PartName)
                .ToListAsync();

            return new InvoiceCreateViewModel
            {
                BookingId = booking.Id,
                BookingNumber = booking.BookingNumber ?? $"SB{booking.Id:D4}",
                LabourCharge = labourCharge,
                GSTPercentage = 18m,
                DiscountPercentage = 0m,
                SparePartsUsed = spareParts.Select(s => new InvoiceItemInputViewModel
                {
                    SparePartId = s.Id,
                    SparePartName = s.PartName,
                    UnitPrice = s.UnitPrice,
                    AvailableStock = s.StockQuantity,
                    QuantityUsed = 0
                }).ToList()
            };
        }

        // ============================================================
        // CREATE INVOICE
        // ============================================================
        public async Task<int> CreateAsync(InvoiceCreateViewModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            if (model.BookingId <= 0)
            {
                throw new InvalidOperationException("Invalid booking.");
            }

            if (model.GSTPercentage < 0 || model.GSTPercentage > 100)
            {
                throw new InvalidOperationException("GST percentage must be between 0 and 100.");
            }

            if (model.DiscountPercentage < 0 || model.DiscountPercentage > 100)
            {
                throw new InvalidOperationException("Discount percentage must be between 0 and 100.");
            }

            var booking = await _context.ServiceBookings
                .Include(b => b.ServiceBookingDetails)
                .Include(b => b.Invoice)
                .FirstOrDefaultAsync(b => b.Id == model.BookingId && !b.IsDeleted);

            if (booking == null)
            {
                throw new KeyNotFoundException("Booking not found.");
            }

            if (booking.Status != BookingStatus.Completed)
            {
                throw new InvalidOperationException("An invoice can only be generated for a completed booking.");
            }

            if (booking.Invoice != null && !booking.Invoice.IsDeleted)
            {
                throw new InvalidOperationException("This booking already has an invoice.");
            }

            var submittedParts = model.SparePartsUsed?
                .Where(p => p != null && p.SparePartId > 0 && p.QuantityUsed > 0)
                .ToList() ?? new List<InvoiceItemInputViewModel>();

            var duplicatePartIds = submittedParts
                .GroupBy(p => p.SparePartId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicatePartIds.Any())
            {
                throw new InvalidOperationException("The same spare part was submitted more than once.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var labourCharge = booking.ServiceBookingDetails?
                    .Where(d => !d.IsDeleted)
                    .Sum(d => d.Price * d.Quantity) ?? 0m;

                decimal sparePartsTotal = 0m;
                var invoiceItems = new List<InvoiceItem>();

                foreach (var submittedPart in submittedParts)
                {
                    if (submittedPart.QuantityUsed <= 0) continue;

                    var sparePart = await _context.SpareParts
                        .FirstOrDefaultAsync(s => s.Id == submittedPart.SparePartId && !s.IsDeleted && s.IsActive);

                    if (sparePart == null)
                    {
                        throw new KeyNotFoundException($"Spare part with ID {submittedPart.SparePartId} was not found.");
                    }

                    if (submittedPart.QuantityUsed > sparePart.StockQuantity)
                    {
                        throw new InvalidOperationException(
                            $"Not enough stock for '{sparePart.PartName}'. Available: {sparePart.StockQuantity}, requested: {submittedPart.QuantityUsed}.");
                    }

                    var unitPrice = sparePart.UnitPrice;
                    var lineAmount = unitPrice * submittedPart.QuantityUsed;
                    sparePartsTotal += lineAmount;

                    sparePart.StockQuantity -= submittedPart.QuantityUsed;
                    sparePart.ModifiedOn = DateTime.UtcNow;

                    invoiceItems.Add(new InvoiceItem
                    {
                        SparePartId = sparePart.Id,
                        Quantity = submittedPart.QuantityUsed,
                        UnitPrice = unitPrice,
                        TotalAmount = lineAmount,
                        CreatedOn = DateTime.UtcNow,
                        IsDeleted = false
                    });
                }

                var subTotal = labourCharge + sparePartsTotal;
                var discountAmount = Math.Round(subTotal * (model.DiscountPercentage / 100m), 2);
                var discountedSubTotal = subTotal - discountAmount;
                var gstAmount = Math.Round(discountedSubTotal * (model.GSTPercentage / 100m), 2);
                var grandTotal = discountedSubTotal + gstAmount;

                var lastInvoice = await _context.Invoices.OrderByDescending(i => i.Id).FirstOrDefaultAsync();
                var nextNumber = (lastInvoice?.Id ?? 0) + 1;
                var invoiceNumber = $"INV{nextNumber:D4}";

                var invoice = new Invoice
                {
                    InvoiceNumber = invoiceNumber,
                    BookingId = booking.Id,
                    LabourCharge = labourCharge,
                    SparePartsTotal = sparePartsTotal,
                    GSTPercentage = model.GSTPercentage,
                    GSTAmount = gstAmount,
                    Discount = discountAmount,
                    GrandTotal = grandTotal,
                    Remarks = model.Remarks,
                    IsPaid = model.IsPaid,
                    InvoiceItems = invoiceItems,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return invoice.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // ============================================================
        // GET INVOICES WITH OUTSTANDING BALANCE (FIXED)
        // Used by PaymentController -> SelectInvoice
        // ============================================================
        public async Task<List<InvoiceListViewModel>> GetInvoicesWithOutstandingBalanceAsync()
        {
            var invoices = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Payments)
                .Include(i => i.ServiceBooking)
                    .ThenInclude(sb => sb.Customer)
                .Include(i => i.ServiceBooking)
                    .ThenInclude(sb => sb.Vehicle)
                .Where(i => !i.IsDeleted)
                .OrderByDescending(i => i.CreatedOn)
                .ToListAsync();

            var result = invoices
                .Select(i =>
                {
                    var amountPaid = i.Payments?
                        .Where(p => !p.IsDeleted)
                        .Sum(p => p.AmountPaid) ?? 0m;

                    return new InvoiceListViewModel
                    {
                        Id = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        BookingId = i.BookingId,
                        CustomerName = i.ServiceBooking?.Customer?.FullName ?? "N/A",
                        VehicleRegistrationNumber = i.ServiceBooking?.Vehicle?.RegistrationNumber ?? "N/A",
                        GrandTotal = i.GrandTotal,
                        AmountPaid = amountPaid,
                        IsPaid = amountPaid >= i.GrandTotal,
                        CreatedOn = i.CreatedOn
                    };
                })
                .Where(x => x.Balance > 0)
                .ToList();

            return result;
        }

        // ============================================================
        // GET BY ID with Customer and Vehicle Details
        // ============================================================
        public async Task<InvoiceDetailsViewModel?> GetByIdAsync(int id)
        {
            var entity = await _context.Invoices
                .Include(i => i.ServiceBooking)
                    .ThenInclude(sb => sb.Customer)
                .Include(i => i.ServiceBooking)
                    .ThenInclude(sb => sb.Vehicle)
                .Include(i => i.ServiceBooking)
                    .ThenInclude(sb => sb.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.SparePart)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (entity == null)
                return null;

            var model = _mapper.Map<InvoiceDetailsViewModel>(entity);

            var amountPaid = entity.Payments?
                .Where(p => !p.IsDeleted)
                .Sum(p => p.AmountPaid) ?? 0m;

            model.AmountPaid = amountPaid;
            model.Balance = entity.GrandTotal - amountPaid;
            model.CustomerName = entity.ServiceBooking?.Customer?.FullName ?? "N/A";
            model.VehicleRegistrationNumber = entity.ServiceBooking?.Vehicle?.RegistrationNumber ?? "N/A";
            model.Email = entity.ServiceBooking?.Customer?.Email;
            model.PhoneNumber = entity.ServiceBooking?.Customer?.PhoneNumber;

            return model;
        }

        // ============================================================
        // PAGINATED INVOICES
        // ============================================================
        public async Task<(IEnumerable<InvoiceListViewModel> Items, int TotalRecords)> GetPaginatedAsync(
            int page,
            int pageSize,
            string? search = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var query = _context.Invoices
                .Include(i => i.ServiceBooking)
                    .ThenInclude(sb => sb.Customer)
                .Include(i => i.ServiceBooking)
                    .ThenInclude(sb => sb.Vehicle)
                .Include(i => i.Payments)
                .Where(i => !i.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i =>
                    i.InvoiceNumber.Contains(search) ||
                    i.ServiceBooking.Customer.FullName.Contains(search) ||
                    i.ServiceBooking.Vehicle.RegistrationNumber.Contains(search));
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(i => i.CreatedOn >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                var endDate = dateTo.Value.Date.AddDays(1);
                query = query.Where(i => i.CreatedOn < endDate);
            }

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderByDescending(i => i.CreatedOn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModels = _mapper.Map<IEnumerable<InvoiceListViewModel>>(items);

            return (viewModels, totalRecords);
        }

        // ============================================================
        // PRINT
        // ============================================================
        public async Task<InvoicePrintViewModel?> GetPrintAsync(int id)
        {
            var entity = await _context.Invoices
                .Include(i => i.ServiceBooking)
                    .ThenInclude(sb => sb.Customer)
                .Include(i => i.ServiceBooking)
                    .ThenInclude(sb => sb.Vehicle)
                .Include(i => i.ServiceBooking)
                    .ThenInclude(sb => sb.ServiceBookingDetails)
                        .ThenInclude(d => d.ServiceType)
                .Include(i => i.InvoiceItems)
                    .ThenInclude(ii => ii.SparePart)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (entity == null)
                return null;

            var model = _mapper.Map<InvoicePrintViewModel>(entity);

            var subtotal = entity.LabourCharge + entity.SparePartsTotal;
            model.DiscountPercentage = subtotal > 0 ? (entity.Discount / subtotal) * 100 : 0;
            model.DiscountAmount = entity.Discount;
            model.AmountPaid = entity.Payments.Where(p => !p.IsDeleted).Sum(p => p.AmountPaid);
            model.AmountInWords = NumberToWords(model.GrandTotal);

            return model;
        }

        // ============================================================
        // GET FOR EDIT
        // ============================================================
        public async Task<InvoiceUpdateViewModel?> GetForEditAsync(int id)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (invoice == null)
                return null;

            var subtotal = invoice.LabourCharge + invoice.SparePartsTotal;
            var discountPercentage = subtotal > 0 ? (invoice.Discount / subtotal) * 100 : 0;

            return new InvoiceUpdateViewModel
            {
                Id = invoice.Id,
                DiscountPercentage = Math.Round(discountPercentage, 2),
                Remarks = invoice.Remarks,
                IsPaid = invoice.IsPaid
            };
        }

        // ============================================================
        // UPDATE
        // ============================================================
        public async Task UpdateAsync(InvoiceUpdateViewModel model)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == model.Id && !i.IsDeleted)
                ?? throw new KeyNotFoundException("Invoice not found.");

            if (model.DiscountPercentage.HasValue)
            {
                var subtotal = invoice.LabourCharge + invoice.SparePartsTotal;
                var newDiscount = subtotal * (model.DiscountPercentage.Value / 100m);
                var discountedSubtotal = subtotal - newDiscount;
                var newGstAmount = Math.Round(discountedSubtotal * (invoice.GSTPercentage / 100m), 2);
                var newGrandTotal = discountedSubtotal + newGstAmount;

                invoice.Discount = newDiscount;
                invoice.GSTAmount = newGstAmount;
                invoice.GrandTotal = newGrandTotal;
            }

            invoice.Remarks = model.Remarks;
            invoice.IsPaid = model.IsPaid;
            invoice.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ============================================================
        // SOFT DELETE
        // ============================================================
        public async Task<bool> SoftDeleteAsync(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceItems)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (invoice == null)
                return false;

            if (invoice.Payments.Any(p => !p.IsDeleted))
            {
                throw new InvalidOperationException("Cannot delete an invoice that has payments.");
            }

            invoice.IsDeleted = true;
            invoice.ModifiedOn = DateTime.UtcNow;

            foreach (var item in invoice.InvoiceItems)
            {
                item.IsDeleted = true;
                item.ModifiedOn = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // RESTORE
        // ============================================================
        public async Task<bool> RestoreAsync(int id)
        {
            var invoice = await _context.Invoices
                .IgnoreQueryFilters()
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == id && i.IsDeleted);

            if (invoice == null)
                return false;

            invoice.IsDeleted = false;
            invoice.ModifiedOn = DateTime.UtcNow;

            foreach (var item in invoice.InvoiceItems)
            {
                item.IsDeleted = false;
                item.ModifiedOn = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // GET DELETED
        // ============================================================
        public async Task<List<InvoiceListViewModel>> GetDeletedAsync()
        {
            var invoices = await _context.Invoices
                .IgnoreQueryFilters()
                .Include(i => i.ServiceBooking)
                    .ThenInclude(sb => sb.Customer)
                .Include(i => i.ServiceBooking)
                    .ThenInclude(sb => sb.Vehicle)
                .Include(i => i.Payments)
                .Where(i => i.IsDeleted)
                .OrderByDescending(i => i.ModifiedOn)
                .ToListAsync();

            return _mapper.Map<List<InvoiceListViewModel>>(invoices);
        }

        // ============================================================
        // MARK PAID
        // ============================================================
        public async Task<bool> MarkAsPaidAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

            if (invoice == null)
                return false;

            invoice.IsPaid = true;
            invoice.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // MARK UNPAID
        // ============================================================
        public async Task<bool> MarkAsUnpaidAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

            if (invoice == null)
                return false;

            invoice.IsPaid = false;
            invoice.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // ============================================================
        // EXISTS
        // ============================================================
        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Invoices
                .AnyAsync(i => i.Id == id && !i.IsDeleted);
        }

        // ============================================================
        // BOOKING INVOICED
        // ============================================================
        public async Task<bool> IsBookingInvoicedAsync(int bookingId)
        {
            return await _context.Invoices
                .AnyAsync(i => i.BookingId == bookingId && !i.IsDeleted);
        }

        // ============================================================
        // GET INVOICE BY BOOKING
        // ============================================================
        public async Task<Invoice?> GetInvoiceByBookingIdAsync(int bookingId)
        {
            return await _context.Invoices
                .Include(i => i.InvoiceItems)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.BookingId == bookingId && !i.IsDeleted);
        }

        // ============================================================
        // STATISTICS
        // ============================================================
        public async Task<InvoiceStatisticsViewModel> GetStatisticsAsync(
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var query = _context.Invoices
                .Include(i => i.Payments)
                .Where(i => !i.IsDeleted);

            if (dateFrom.HasValue)
            {
                query = query.Where(i => i.CreatedOn >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                var endDate = dateTo.Value.Date.AddDays(1);
                query = query.Where(i => i.CreatedOn < endDate);
            }

            var invoices = await query.ToListAsync();

            var stats = new InvoiceStatisticsViewModel
            {
                TotalInvoices = invoices.Count,
                PaidInvoices = invoices.Count(i => i.IsPaid),
                UnpaidInvoices = invoices.Count(i => !i.IsPaid),
                TotalRevenue = invoices.Sum(i => i.GrandTotal),
                TotalCollected = invoices.Sum(i => i.Payments.Where(p => !p.IsDeleted).Sum(p => p.AmountPaid)),
                TodaysRevenue = invoices.Where(i => i.CreatedOn.Date == DateTime.UtcNow.Date).Sum(i => i.GrandTotal),
                ThisMonthRevenue = invoices.Where(i => i.CreatedOn.Month == DateTime.UtcNow.Month && i.CreatedOn.Year == DateTime.UtcNow.Year).Sum(i => i.GrandTotal)
            };

            stats.TotalOutstanding = stats.TotalRevenue - stats.TotalCollected;
            stats.AverageInvoiceValue = stats.TotalInvoices > 0 ? stats.TotalRevenue / stats.TotalInvoices : 0;

            return stats;
        }

        // ============================================================
        // NUMBER TO WORDS
        // ============================================================
        private string NumberToWords(decimal number)
        {
            return $"{number:N2} only";
        }
    }
}