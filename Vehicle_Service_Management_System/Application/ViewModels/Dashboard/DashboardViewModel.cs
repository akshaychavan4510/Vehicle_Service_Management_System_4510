

using ServiceBookingEntity = Vehicle_Service_Management_System.Domain.Entities.ServiceBooking;
using PaymentEntity = Vehicle_Service_Management_System.Domain.Entities.Payment;

namespace Vehicle_Service_Management_System.Application.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public int Customers { get; set; }
        public int Vehicles { get; set; }
        public int Mechanics { get; set; }
        public int Bookings { get; set; }
        public int JobCards { get; set; }
        public int Invoices { get; set; }
        public int Payments { get; set; }
        public decimal Revenue { get; set; }

        public List<ServiceBookingEntity> RecentBookings { get; set; } = new();

        public List<PaymentEntity> RecentPayments { get; set; } = new();
    }
}