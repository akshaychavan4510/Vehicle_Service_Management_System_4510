using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Service_Management_System.Infrastructure.Data;
using Vehicle_Service_Management_System.Application.ViewModels.Dashboard;
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            Customers = await _context.Customers.CountAsync(),
            Vehicles = await _context.Vehicles.CountAsync(),
            Mechanics = await _context.Mechanics.CountAsync(),
            Bookings = await _context.ServiceBookings.CountAsync(),
            JobCards = await _context.JobCards.CountAsync(),
            Invoices = await _context.Invoices.CountAsync(),
            Payments = await _context.Payments.CountAsync(),

            Revenue = await _context.Payments.SumAsync(x => x.AmountPaid),

            RecentBookings = await _context.ServiceBookings
                                    .OrderByDescending(x => x.Id)
                                    .Take(5)
                                    .ToListAsync(),

            RecentPayments = await _context.Payments
                                    .OrderByDescending(x => x.Id)
                                    .Take(5)
                                    .ToListAsync()
        };

        return View(model);
    }
}