using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Vehicle_Service_Management_System.Application.Services;
using Vehicle_Service_Management_System.Domain.Enums;

namespace Vehicle_Service_Management_System.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ReportService _reportService;

        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: /Report
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Report/Revenue
        public async Task<IActionResult> Revenue(DateTime? from, DateTime? to)
        {
            var model = await _reportService.GetRevenueReportAsync(from, to);
            return View(model);
        }

        // GET: /Report/Bookings
        public async Task<IActionResult> Bookings(DateTime? from, DateTime? to, BookingStatus? status)
        {
            var model = await _reportService.GetBookingReportAsync(from, to, status);
            return View(model);
        }

        // GET: /Report/Customers
        public async Task<IActionResult> Customers(DateTime? from, DateTime? to)
        {
            var model = await _reportService.GetCustomerReportAsync(from, to);
            return View(model);
        }

        // GET: /Report/Vehicles
        public async Task<IActionResult> Vehicles(DateTime? from, DateTime? to)
        {
            var model = await _reportService.GetVehicleReportAsync(from, to);
            return View(model);
        }

        // GET: /Report/Mechanics
        public async Task<IActionResult> Mechanics(DateTime? from, DateTime? to)
        {
            var model = await _reportService.GetMechanicReportAsync(from, to);
            return View(model);
        }

        // GET: /Report/Gst
        public async Task<IActionResult> Gst(DateTime? from, DateTime? to)
        {
            var model = await _reportService.GetGstReportAsync(from, to);
            return View(model);
        }

        // GET: /Report/ExportCsv?type=revenue&from=&to=
        // Generic CSV export so every report can be opened in Excel without needing
        // an extra NuGet package. Swap this for ClosedXML/EPPlus later if you want a
        // real formatted .xlsx file.
        public async Task<IActionResult> ExportCsv(string type, DateTime? from, DateTime? to, BookingStatus? status)
        {
            var sb = new StringBuilder();

            switch (type?.ToLowerInvariant())
            {
                case "revenue":
                    {
                        var m = await _reportService.GetRevenueReportAsync(from, to);
                        sb.AppendLine("Month,Invoiced,Collected");
                        foreach (var row in m.MonthlyTrend)
                            sb.AppendLine($"{row.MonthLabel},{row.Invoiced},{row.Collected}");
                        sb.AppendLine();
                        sb.AppendLine($"Total Invoiced,{m.TotalInvoiced}");
                        sb.AppendLine($"Total Collected,{m.TotalCollected}");
                        sb.AppendLine($"Total Outstanding,{m.TotalOutstanding}");
                        sb.AppendLine($"Total GST,{m.TotalGST}");
                        break;
                    }
                case "bookings":
                    {
                        var m = await _reportService.GetBookingReportAsync(from, to, status);
                        sb.AppendLine("BookingNumber,BookingDate,Customer,Vehicle,ServiceType,Mechanic,Status,Amount");
                        foreach (var row in m.Bookings)
                            sb.AppendLine($"{Csv(row.BookingNumber)},{row.BookingDate:yyyy-MM-dd},{Csv(row.CustomerName)},{Csv(row.VehicleReg)},{Csv(row.ServiceTypeName)},{Csv(row.MechanicName)},{row.Status},{row.Amount}");
                        break;
                    }
                case "customers":
                    {
                        var m = await _reportService.GetCustomerReportAsync(from, to, topN: 1000);
                        sb.AppendLine("CustomerName,Phone,TotalBookings,TotalSpent,LastVisit");
                        foreach (var row in m.TopCustomers)
                            sb.AppendLine($"{Csv(row.FullName)},{Csv(row.PhoneNumber)},{row.TotalBookings},{row.TotalSpent},{row.LastVisit:yyyy-MM-dd}");
                        break;
                    }
                case "vehicles":
                    {
                        var m = await _reportService.GetVehicleReportAsync(from, to, topN: 1000);
                        sb.AppendLine("RegistrationNumber,VehicleName,Customer,ServiceCount,TotalSpent,LastServiceDate");
                        foreach (var row in m.MostServiced)
                            sb.AppendLine($"{Csv(row.RegistrationNumber)},{Csv(row.VehicleName)},{Csv(row.CustomerName)},{row.ServiceCount},{row.TotalSpent},{row.LastServiceDate:yyyy-MM-dd}");
                        break;
                    }
                case "mechanics":
                    {
                        var m = await _reportService.GetMechanicReportAsync(from, to);
                        sb.AppendLine("MechanicName,Specialization,TotalBookings,CompletedBookings,RevenueGenerated");
                        foreach (var row in m.Mechanics)
                            sb.AppendLine($"{Csv(row.FullName)},{Csv(row.Specialization ?? string.Empty)},{row.TotalBookings},{row.CompletedBookings},{row.RevenueGenerated}");
                        break;
                    }
                case "gst":
                    {
                        var m = await _reportService.GetGstReportAsync(from, to);
                        sb.AppendLine("InvoiceNumber,InvoiceDate,Customer,Vehicle,TaxableAmount,GSTPercentage,GSTAmount,Discount,GrandTotal");
                        foreach (var row in m.Invoices)
                            sb.AppendLine($"{Csv(row.InvoiceNumber)},{row.InvoiceDate:yyyy-MM-dd},{Csv(row.CustomerName)},{Csv(row.VehicleReg)},{row.TaxableAmount},{row.GSTPercentage},{row.GSTAmount},{row.Discount},{row.GrandTotal}");
                        sb.AppendLine();
                        sb.AppendLine($"Total Taxable,{m.TotalTaxable}");
                        sb.AppendLine($"Total GST,{m.TotalGST}");
                        sb.AppendLine($"Total Grand,{m.TotalGrand}");
                        break;
                    }
                default:
                    return BadRequest("Unknown report type.");
            }

            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
            var fileName = $"{type}-report-{DateTime.Now:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", fileName);
        }

        // Minimal CSV field escaping: wrap in quotes if it contains a comma, quote or newline.
        private static string Csv(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}