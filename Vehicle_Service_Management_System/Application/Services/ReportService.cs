using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Application.ViewModels.Report;
using Vehicle_Service_Management_System.Domain.Enums;
using Vehicle_Service_Management_System.Infrastructure.Data;

namespace Vehicle_Service_Management_System.Application.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        private static (DateTime? from, DateTime? toExclusive) NormalizeRange(DateTime? from, DateTime? to)
        {
            DateTime? toExclusive = to?.Date.AddDays(1);
            return (from?.Date, toExclusive);
        }

        // ───────────────────────── Revenue report ─────────────────────────
        public async Task<RevenueReportViewModel> GetRevenueReportAsync(DateTime? from, DateTime? to)
        {
            var (dFrom, dTo) = NormalizeRange(from, to);

            var invoiceQuery = _context.Invoices.AsQueryable();
            if (dFrom.HasValue) invoiceQuery = invoiceQuery.Where(i => i.CreatedOn >= dFrom.Value);
            if (dTo.HasValue) invoiceQuery = invoiceQuery.Where(i => i.CreatedOn < dTo.Value);

            var paymentQuery = _context.Payments.AsQueryable();
            if (dFrom.HasValue) paymentQuery = paymentQuery.Where(p => p.PaymentDate >= dFrom.Value);
            if (dTo.HasValue) paymentQuery = paymentQuery.Where(p => p.PaymentDate < dTo.Value);

            var totalInvoiced = await invoiceQuery.SumAsync(i => (decimal?)i.GrandTotal) ?? 0m;
            var totalGst = await invoiceQuery.SumAsync(i => (decimal?)i.GSTAmount) ?? 0m;
            var invoiceCount = await invoiceQuery.CountAsync();

            var totalCollected = await paymentQuery.SumAsync(p => (decimal?)p.AmountPaid) ?? 0m;
            var paymentCount = await paymentQuery.CountAsync();

            // Monthly trend
            var invoicedByMonth = await invoiceQuery
                .GroupBy(i => new { i.CreatedOn.Year, i.CreatedOn.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.GrandTotal) })
                .ToListAsync();

            var collectedByMonth = await paymentQuery
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.AmountPaid) })
                .ToListAsync();

            var monthlyTrend = invoicedByMonth
                .Select(x => new { x.Year, x.Month })
                .Union(collectedByMonth.Select(x => new { x.Year, x.Month }))
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .Select(key => new MonthlyRevenuePoint
                {
                    Year = key.Year,
                    Month = key.Month,
                    Invoiced = invoicedByMonth.FirstOrDefault(x => x.Year == key.Year && x.Month == key.Month)?.Total ?? 0m,
                    Collected = collectedByMonth.FirstOrDefault(x => x.Year == key.Year && x.Month == key.Month)?.Total ?? 0m
                })
                .ToList();

            var paymentModeBreakdown = await paymentQuery
                .GroupBy(p => p.PaymentMode)
                .Select(g => new { Mode = g.Key, Total = g.Sum(x => x.AmountPaid) })
                .ToDictionaryAsync(g => g.Mode.ToString(), g => g.Total);

            return new RevenueReportViewModel
            {
                DateFrom = from,
                DateTo = to,
                InvoiceCount = invoiceCount,
                PaymentCount = paymentCount,
                TotalInvoiced = totalInvoiced,
                TotalCollected = totalCollected,
                TotalOutstanding = totalInvoiced - totalCollected,
                TotalGST = totalGst,
                MonthlyTrend = monthlyTrend,
                PaymentModeBreakdown = paymentModeBreakdown
            };
        }

        // ───────────────────────── Booking report ─────────────────────────
        public async Task<BookingReportViewModel> GetBookingReportAsync(DateTime? from, DateTime? to, BookingStatus? status)
        {
            var (dFrom, dTo) = NormalizeRange(from, to);

            var query = _context.ServiceBookings
                .Include(b => b.Customer)
                .Include(b => b.Vehicle)
                .Include(b => b.Mechanic)
                .Include(b => b.ServiceBookingDetails)          // ✅ correct
                    .ThenInclude(d => d.ServiceType)            // ✅ correct
                .Include(b => b.Invoice)
                .AsQueryable();

            if (dFrom.HasValue) query = query.Where(b => b.BookingDate >= dFrom.Value);
            if (dTo.HasValue) query = query.Where(b => b.BookingDate < dTo.Value);
            if (status.HasValue) query = query.Where(b => b.Status == status.Value);

            var totalBookings = await query.CountAsync();

            var statusBreakdown = await query
                .GroupBy(b => b.Status)
                .Select(g => new BookingStatusCount { Status = g.Key.ToString(), Count = g.Count() })
                .OrderBy(x => x.Status)
                .ToListAsync();

            // ✅ Service type breakdown via ServiceBookingDetails
            var byServiceType = await query
                .SelectMany(b => b.ServiceBookingDetails)      // flatten details
                .Where(d => !d.IsDeleted)
                .GroupBy(d => d.ServiceType.ServiceName)
                .Select(g => new ServiceTypeBookingRow
                {
                    ServiceTypeName = g.Key,
                    BookingCount = g.Count(),
                    Revenue = g.Sum(d => d.ServiceBooking.Invoice != null ? d.ServiceBooking.Invoice.GrandTotal : 0m)
                })
                .OrderByDescending(x => x.BookingCount)
                .ToListAsync();

            // ✅ Booking list – service names concatenated
            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .Take(300)
                .Select(b => new BookingListRow
                {
                    BookingNumber = b.BookingNumber,
                    BookingDate = b.BookingDate,
                    CustomerName = b.Customer.FullName,
                    VehicleReg = b.Vehicle.RegistrationNumber,
                    ServiceTypeName = string.Join(", ", b.ServiceBookingDetails
                        .Where(d => !d.IsDeleted)
                        .Select(d => d.ServiceType.ServiceName)),   // ✅ concatenated
                    MechanicName = b.Mechanic != null ? b.Mechanic.FullName : "Unassigned",
                    Status = b.Status.ToString(),
                    Amount = b.Invoice != null ? b.Invoice.GrandTotal : 0m
                })
                .ToListAsync();

            return new BookingReportViewModel
            {
                DateFrom = from,
                DateTo = to,
                Status = status,
                TotalBookings = totalBookings,
                StatusBreakdown = statusBreakdown,
                ByServiceType = byServiceType,
                Bookings = bookings
            };
        }

        // ───────────────────────── Customer report ─────────────────────────
        public async Task<CustomerReportViewModel> GetCustomerReportAsync(DateTime? from, DateTime? to, int topN = 20)
        {
            var (dFrom, dTo) = NormalizeRange(from, to);

            var totalCustomers = await _context.Customers.CountAsync();

            var newCustomersQuery = _context.Customers.AsQueryable();
            if (dFrom.HasValue) newCustomersQuery = newCustomersQuery.Where(c => c.CreatedOn >= dFrom.Value);
            if (dTo.HasValue) newCustomersQuery = newCustomersQuery.Where(c => c.CreatedOn < dTo.Value);
            var newCustomers = await newCustomersQuery.CountAsync();

            var bookingQuery = _context.ServiceBookings
                .Include(b => b.Invoice)
                .AsQueryable();
            if (dFrom.HasValue) bookingQuery = bookingQuery.Where(b => b.BookingDate >= dFrom.Value);
            if (dTo.HasValue) bookingQuery = bookingQuery.Where(b => b.BookingDate < dTo.Value);

            var topCustomers = await bookingQuery
                .GroupBy(b => new { b.CustomerId, b.Customer.FullName, b.Customer.PhoneNumber })
                .Select(g => new CustomerReportRow
                {
                    FullName = g.Key.FullName,
                    PhoneNumber = g.Key.PhoneNumber,
                    TotalBookings = g.Count(),
                    TotalSpent = g.Sum(b => b.Invoice != null ? b.Invoice.GrandTotal : 0m),
                    LastVisit = g.Max(b => b.BookingDate)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(topN)
                .ToListAsync();

            return new CustomerReportViewModel
            {
                DateFrom = from,
                DateTo = to,
                TotalCustomers = totalCustomers,
                NewCustomers = newCustomers,
                TopCustomers = topCustomers
            };
        }

        // ───────────────────────── Vehicle report ─────────────────────────
        public async Task<VehicleReportViewModel> GetVehicleReportAsync(DateTime? from, DateTime? to, int topN = 20)
        {
            var (dFrom, dTo) = NormalizeRange(from, to);

            var totalVehicles = await _context.Vehicles.CountAsync();

            var byBrand = await _context.Vehicles
                .GroupBy(v => v.VehicleBrand.BrandName)
                .Select(g => new VehicleBrandCount { BrandName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var byType = await _context.Vehicles
                .GroupBy(v => v.VehicleType.TypeName)
                .Select(g => new VehicleTypeCount { TypeName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var bookingQuery = _context.ServiceBookings
                .Include(b => b.Invoice)
                .AsQueryable();
            if (dFrom.HasValue) bookingQuery = bookingQuery.Where(b => b.BookingDate >= dFrom.Value);
            if (dTo.HasValue) bookingQuery = bookingQuery.Where(b => b.BookingDate < dTo.Value);

            var mostServiced = await bookingQuery
                .GroupBy(b => new
                {
                    b.VehicleId,
                    b.Vehicle.RegistrationNumber,
                    b.Vehicle.VehicleName,
                    b.Customer.FullName
                })
                .Select(g => new VehicleServiceRow
                {
                    RegistrationNumber = g.Key.RegistrationNumber,
                    VehicleName = g.Key.VehicleName,
                    CustomerName = g.Key.FullName,
                    ServiceCount = g.Count(),
                    TotalSpent = g.Sum(b => b.Invoice != null ? b.Invoice.GrandTotal : 0m),
                    LastServiceDate = g.Max(b => b.BookingDate)
                })
                .OrderByDescending(x => x.ServiceCount)
                .Take(topN)
                .ToListAsync();

            return new VehicleReportViewModel
            {
                DateFrom = from,
                DateTo = to,
                TotalVehicles = totalVehicles,
                ByBrand = byBrand,
                ByType = byType,
                MostServiced = mostServiced
            };
        }

        // ───────────────────────── Mechanic report ─────────────────────────
        public async Task<MechanicReportViewModel> GetMechanicReportAsync(DateTime? from, DateTime? to)
        {
            var (dFrom, dTo) = NormalizeRange(from, to);

            var bookingQuery = _context.ServiceBookings
                .Include(b => b.Invoice)
                .Where(b => b.MechanicId != null)
                .AsQueryable();
            if (dFrom.HasValue) bookingQuery = bookingQuery.Where(b => b.BookingDate >= dFrom.Value);
            if (dTo.HasValue) bookingQuery = bookingQuery.Where(b => b.BookingDate < dTo.Value);

            var performance = await bookingQuery
                .GroupBy(b => new
                {
                    b.MechanicId,
                    b.Mechanic!.FullName,
                    b.Mechanic!.Specialization
                })
                .Select(g => new MechanicPerformanceRow
                {
                    FullName = g.Key.FullName,
                    Specialization = g.Key.Specialization,
                    TotalBookings = g.Count(),
                    CompletedBookings = g.Count(b => b.Status == BookingStatus.Completed),
                    RevenueGenerated = g.Sum(b => b.Invoice != null ? b.Invoice.GrandTotal : 0m)
                })
                .OrderByDescending(x => x.RevenueGenerated)
                .ToListAsync();

            var namesWithBookings = performance.Select(p => p.FullName).ToHashSet();
            var idle = await _context.Mechanics
                .Where(m => !namesWithBookings.Contains(m.FullName))
                .Select(m => new MechanicPerformanceRow
                {
                    FullName = m.FullName,
                    Specialization = m.Specialization,
                    TotalBookings = 0,
                    CompletedBookings = 0,
                    RevenueGenerated = 0m
                })
                .ToListAsync();

            performance.AddRange(idle);

            return new MechanicReportViewModel
            {
                DateFrom = from,
                DateTo = to,
                Mechanics = performance
            };
        }

        // ───────────────────────── GST report ─────────────────────────
        public async Task<GstReportViewModel> GetGstReportAsync(DateTime? from, DateTime? to)
        {
            var (dFrom, dTo) = NormalizeRange(from, to);

            var query = _context.Invoices
                .Include(i => i.ServiceBooking)
                    .ThenInclude(b => b.Customer)
                .Include(i => i.ServiceBooking)
                    .ThenInclude(b => b.Vehicle)
                .AsQueryable();

            if (dFrom.HasValue) query = query.Where(i => i.CreatedOn >= dFrom.Value);
            if (dTo.HasValue) query = query.Where(i => i.CreatedOn < dTo.Value);

            var rows = await query
                .OrderBy(i => i.CreatedOn)
                .Select(i => new GstInvoiceRow
                {
                    InvoiceNumber = i.InvoiceNumber,
                    InvoiceDate = i.CreatedOn,
                    CustomerName = i.ServiceBooking.Customer.FullName,
                    VehicleReg = i.ServiceBooking.Vehicle.RegistrationNumber,
                    TaxableAmount = i.LabourCharge + i.SparePartsTotal - i.Discount,
                    GSTPercentage = i.GSTPercentage,
                    GSTAmount = i.GSTAmount,
                    Discount = i.Discount,
                    GrandTotal = i.GrandTotal
                })
                .ToListAsync();

            var totalTaxable = rows.Sum(r => r.TaxableAmount);
            var totalGST = rows.Sum(r => r.GSTAmount);
            var totalGrand = rows.Sum(r => r.GrandTotal);

            return new GstReportViewModel
            {
                DateFrom = from,
                DateTo = to,
                Invoices = rows,
                TotalTaxable = totalTaxable,
                TotalGST = totalGST,
                TotalGrand = totalGrand
            };
        }
    }
}