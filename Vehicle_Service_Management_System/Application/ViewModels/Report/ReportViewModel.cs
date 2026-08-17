using Vehicle_Service_Management_System.Domain.Enums;

namespace Vehicle_Service_Management_System.Application.ViewModels.Report
{
    // ───────────────────────── Shared filter ─────────────────────────
    public class ReportFilterViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    // ───────────────────────── Revenue report ─────────────────────────
    public class MonthlyRevenuePoint
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthLabel => new DateTime(Year, Month, 1).ToString("MMM yyyy");
        public decimal Invoiced { get; set; }
        public decimal Collected { get; set; }
    }

    public class RevenueReportViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public int InvoiceCount { get; set; }
        public int PaymentCount { get; set; }

        public decimal TotalInvoiced { get; set; }
        public decimal TotalCollected { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal TotalGST { get; set; }

        public List<MonthlyRevenuePoint> MonthlyTrend { get; set; } = new();
        public Dictionary<string, decimal> PaymentModeBreakdown { get; set; } = new();
    }

    // ───────────────────────── Booking report ─────────────────────────
    public class BookingStatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ServiceTypeBookingRow
    {
        public string ServiceTypeName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class BookingListRow
    {
        public string BookingNumber { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehicleReg { get; set; } = string.Empty;
        public string ServiceTypeName { get; set; } = string.Empty;
        public string MechanicName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class BookingReportViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public BookingStatus? Status { get; set; }

        public int TotalBookings { get; set; }

        public List<BookingStatusCount> StatusBreakdown { get; set; } = new();
        public List<ServiceTypeBookingRow> ByServiceType { get; set; } = new();
        public List<BookingListRow> Bookings { get; set; } = new();
    }

    // ───────────────────────── Customer report ─────────────────────────
    public class CustomerReportRow
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastVisit { get; set; }
    }

    public class CustomerReportViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }

        public List<CustomerReportRow> TopCustomers { get; set; } = new();
    }

    // ───────────────────────── Vehicle report ─────────────────────────
    public class VehicleBrandCount
    {
        public string BrandName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class VehicleTypeCount
    {
        public string TypeName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class VehicleServiceRow
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public string VehicleName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int ServiceCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastServiceDate { get; set; }
    }

    public class VehicleReportViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public int TotalVehicles { get; set; }

        public List<VehicleBrandCount> ByBrand { get; set; } = new();
        public List<VehicleTypeCount> ByType { get; set; } = new();
        public List<VehicleServiceRow> MostServiced { get; set; } = new();
    }

    // ───────────────────────── Mechanic report ─────────────────────────
    public class MechanicPerformanceRow
    {
        public string FullName { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal RevenueGenerated { get; set; }
    }

    public class MechanicReportViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public List<MechanicPerformanceRow> Mechanics { get; set; } = new();
    }

    // ───────────────────────── GST report ─────────────────────────
    public class GstInvoiceRow
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehicleReg { get; set; } = string.Empty;
        public decimal TaxableAmount { get; set; }
        public decimal GSTPercentage { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class GstReportViewModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public List<GstInvoiceRow> Invoices { get; set; } = new();

        public decimal TotalTaxable { get; set; }
        public decimal TotalGST { get; set; }
        public decimal TotalGrand { get; set; }
    }
}