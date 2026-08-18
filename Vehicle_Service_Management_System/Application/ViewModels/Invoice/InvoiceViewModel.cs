using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Vehicle_Service_Management_System.Application.ViewModels.Invoice
{
    // ============================================================
    // CREATE VIEW MODEL
    // ============================================================
    public class InvoiceCreateViewModel
    {
        [Required(ErrorMessage = "Booking is required")]
        [Display(Name = "Booking")]
        public int BookingId { get; set; }

        [Display(Name = "Booking Number")]
        public string BookingNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Labour charge is required")]
        [Range(0, 999999.99, ErrorMessage = "Labour charge must be between 0 and 999,999.99")]
        [Display(Name = "Labour Charge")]
        public decimal LabourCharge { get; set; }

        [Display(Name = "Spare Parts Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal SparePartsTotal { get; set; }

        [Range(0, 100, ErrorMessage = "GST percentage must be between 0 and 100")]
        [Display(Name = "GST Percentage")]
        public decimal GSTPercentage { get; set; } = 18m;

        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
        [Display(Name = "Discount %")]
        public decimal DiscountPercentage { get; set; } = 0m;

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Is Paid")]
        public bool IsPaid { get; set; } = false;

        public List<InvoiceItemInputViewModel> SparePartsUsed { get; set; } = new();

        // Computed properties for display
        public decimal Subtotal => LabourCharge + SparePartsTotal;
        public decimal DiscountAmount => Subtotal * (DiscountPercentage / 100m);
        public decimal DiscountedSubtotal => Subtotal - DiscountAmount;
        public decimal GSTAmount => Math.Round(DiscountedSubtotal * (GSTPercentage / 100m), 2);
        public decimal GrandTotal => DiscountedSubtotal + GSTAmount;
    }

    // ============================================================
    // INVOICE ITEM INPUT VIEW MODEL (for Create form)
    // ============================================================
    public class InvoiceItemInputViewModel
    {
        [Required(ErrorMessage = "Spare part is required")]
        public int SparePartId { get; set; }

        [Display(Name = "Spare Part")]
        public string SparePartName { get; set; } = string.Empty;

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Available Stock")]
        public int AvailableStock { get; set; }

        [Display(Name = "Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        public int QuantityUsed { get; set; } = 0;

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalPrice => UnitPrice * QuantityUsed;
    }

    // ============================================================
    // LIST VIEW MODEL
    // ============================================================
    public class InvoiceListViewModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public int BookingId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string VehicleRegistrationNumber { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance => GrandTotal - AmountPaid;
        public bool IsPaid { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    // ============================================================
    // DETAILS VIEW MODEL
    // ============================================================
    public class InvoiceDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Invoice #")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Display(Name = "Booking #")]
        public int BookingId { get; set; }

        [Display(Name = "Customer")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Vehicle")]
        public string VehicleRegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Services")]
        public string ServicesSummary { get; set; } = string.Empty;

        [Display(Name = "Labour Charge")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal LabourCharge { get; set; }

        [Display(Name = "Spare Parts Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal SparePartsTotal { get; set; }

        [Display(Name = "GST")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal GSTPercentage { get; set; }

        [Display(Name = "GST Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal GSTAmount { get; set; }

        [Display(Name = "Discount %")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal DiscountPercentage { get; set; }

        [Display(Name = "Discount Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Grand Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal GrandTotal { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Payment Status")]
        public bool IsPaid { get; set; }

        [Display(Name = "Created")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Modified")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? ModifiedOn { get; set; }

        [Display(Name = "Total Paid")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AmountPaid { get; set; }

        [Display(Name = "Balance")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Balance { get; set; }

        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public List<InvoiceItemLineViewModel> Items { get; set; } = new();
        public List<PaymentLineViewModel> Payments { get; set; } = new();

        public string PaymentStatus => Balance <= 0 ? "Fully Paid" : "Partial Payment";
        public string StatusBadge => Balance <= 0 ? "bg-success" : "bg-warning";
    }

    // ============================================================
    // INVOICE ITEM LINE VIEW MODEL
    // ============================================================
    public class InvoiceItemLineViewModel
    {
        public int Id { get; set; }
        public int SparePartId { get; set; }

        [Display(Name = "Spare Part")]
        public string SparePartName { get; set; } = string.Empty;

        [Display(Name = "Qty")]
        public int Quantity { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalAmount { get; set; }
    }

    // ============================================================
    // PAYMENT LINE VIEW MODEL
    // ============================================================
    public class PaymentLineViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Payment Mode")]
        public string PaymentMode { get; set; } = string.Empty;

        [Display(Name = "Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AmountPaid { get; set; }

        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime PaymentDate { get; set; }
    }

    // ============================================================
    // UPDATE VIEW MODEL
    // ============================================================
    public class InvoiceUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Range(0, 100, ErrorMessage = "Discount percentage must be between 0 and 100")]
        [Display(Name = "Discount %")]
        public decimal? DiscountPercentage { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Is Paid")]
        public bool IsPaid { get; set; }
    }

    // ============================================================
    // PRINT VIEW MODEL
    // ============================================================
    public class InvoicePrintViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Invoice #")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Display(Name = "Invoice Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime InvoiceDate { get; set; }

        [Display(Name = "Customer")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Address")]
        public string? CustomerAddress { get; set; }

        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Display(Name = "Vehicle")]
        public string VehicleRegistrationNumber { get; set; } = string.Empty;

        [Display(Name = "Booking ID")]
        public int BookingId { get; set; }

        [Display(Name = "Services")]
        public string ServicesSummary { get; set; } = string.Empty;

        [Display(Name = "Labour Charge")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal LabourCharge { get; set; }

        [Display(Name = "Spare Parts Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal SparePartsTotal { get; set; }

        [Display(Name = "GST")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal GSTPercentage { get; set; }

        [Display(Name = "GST Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal GSTAmount { get; set; }

        [Display(Name = "Discount %")]
        [DisplayFormat(DataFormatString = "{0:F2}%")]
        public decimal DiscountPercentage { get; set; }

        [Display(Name = "Discount Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal DiscountAmount { get; set; }

        [Display(Name = "Grand Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal GrandTotal { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Amount Paid")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AmountPaid { get; set; }

        [Display(Name = "Balance")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Balance => GrandTotal - AmountPaid;

        [Display(Name = "Amount in Words")]
        public string AmountInWords { get; set; } = string.Empty;

        public List<InvoiceItemPrintViewModel> Items { get; set; } = new();
    }

    // ============================================================
    // INVOICE ITEM PRINT VIEW MODEL
    // ============================================================
    public class InvoiceItemPrintViewModel
    {
        public int SrNo { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Qty")]
        public int Quantity { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalAmount { get; set; }
    }

    // ============================================================
    // RESPONSE VIEW MODEL
    // ============================================================
    public class InvoiceResponseViewModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal GrandTotal { get; set; }
        public bool IsPaid { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // ============================================================
    // STATISTICS VIEW MODEL
    // ============================================================
    public class InvoiceStatisticsViewModel
    {
        [Display(Name = "Total Invoices")]
        public int TotalInvoices { get; set; }

        [Display(Name = "Paid Invoices")]
        public int PaidInvoices { get; set; }

        [Display(Name = "Unpaid Invoices")]
        public int UnpaidInvoices { get; set; }

        [Display(Name = "Total Revenue")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Total Collected")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalCollected { get; set; }

        [Display(Name = "Total Outstanding")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalOutstanding { get; set; }

        [Display(Name = "Average Invoice Value")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AverageInvoiceValue { get; set; }

        [Display(Name = "Today's Revenue")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TodaysRevenue { get; set; }

        [Display(Name = "This Month Revenue")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal ThisMonthRevenue { get; set; }
    }

    // ============================================================
    // SEARCH VIEW MODEL
    // ============================================================
    public class InvoiceSearchViewModel
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Date From")]
        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Date To")]
        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Display(Name = "Payment Status")]
        public bool? IsPaid { get; set; }

        [Display(Name = "Min Amount")]
        public decimal? MinAmount { get; set; }

        [Display(Name = "Max Amount")]
        public decimal? MaxAmount { get; set; }

        public List<SelectListItem> Customers { get; set; } = new();
    }
}