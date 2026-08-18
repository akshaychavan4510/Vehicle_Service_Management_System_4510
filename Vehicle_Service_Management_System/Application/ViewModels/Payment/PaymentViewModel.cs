using System.ComponentModel.DataAnnotations;
using Vehicle_Service_Management_System.Domain.Enums;

namespace Vehicle_Service_Management_System.Application.ViewModels.Payment
{
    public class PaymentListViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Invoice ID")]
        public int InvoiceId { get; set; }
        [Display(Name = "Invoice Number")]
        public string? InvoiceNumber { get; set; }
        [Display(Name = "Customer")]
        public string? CustomerName { get; set; }
        [Display(Name = "Payment Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime PaymentDate { get; set; }
        [Display(Name = "Payment Mode")]
        public string PaymentMode { get; set; } = string.Empty;
        [Display(Name = "Amount Paid")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AmountPaid { get; set; }
        [Display(Name = "Transaction Reference")]
        public string? TransactionReference { get; set; }
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedOn { get; set; }
    }

    public class PaymentDetailsViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Invoice ID")]
        public int InvoiceId { get; set; }
        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; } = string.Empty;
        [Display(Name = "Customer")]
        public string CustomerName { get; set; } = string.Empty;
        [Display(Name = "Vehicle")]
        public string VehicleNumber { get; set; } = string.Empty;
        [Display(Name = "Payment Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime PaymentDate { get; set; }
        [Display(Name = "Payment Mode")]
        public string PaymentMode { get; set; } = string.Empty;
        [Display(Name = "Amount Paid")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AmountPaid { get; set; }
        [Display(Name = "Transaction Reference")]
        public string? TransactionReference { get; set; }
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedOn { get; set; }
        [Display(Name = "Modified Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? ModifiedOn { get; set; }

        // Invoice Details
        [Display(Name = "Invoice Grand Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal InvoiceGrandTotal { get; set; }
        [Display(Name = "Total Paid")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalPaid { get; set; }
        [Display(Name = "Balance")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Balance => InvoiceGrandTotal - TotalPaid;
        [Display(Name = "Payment Status")]
        public string PaymentStatus => Balance <= 0 ? "Fully Paid" : "Partial Payment";
    }

    public class PaymentFormViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Invoice is required")]
        [Display(Name = "Invoice")]
        public int InvoiceId { get; set; }
        [Display(Name = "Invoice Number")]
        public string? InvoiceNumber { get; set; }
        [Required(ErrorMessage = "Payment date is required")]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Now; // ✅ default added
        [Required(ErrorMessage = "Payment mode is required")]
        [Display(Name = "Payment Mode")]
        public PaymentMode PaymentMode { get; set; }
        [Required(ErrorMessage = "Amount paid is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999.99")]
        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }
        [StringLength(50, ErrorMessage = "Transaction reference cannot exceed 50 characters")]
        [Display(Name = "Transaction Reference")]
        public string? TransactionReference { get; set; }
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        [Display(Name = "Invoice Grand Total")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal InvoiceGrandTotal { get; set; }
        [Display(Name = "Amount Paid So Far")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AmountPaidSoFar { get; set; }
        [Display(Name = "Remaining Balance")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal RemainingBalance => InvoiceGrandTotal - AmountPaidSoFar;
        [Display(Name = "Payment Status")]
        public string PaymentStatus => RemainingBalance <= 0 ? "Fully Paid" : "Partial Payment";
    }

    public class PaymentCreateViewModel
    {
        [Required(ErrorMessage = "Invoice is required")]
        public int InvoiceId { get; set; }

        [Required(ErrorMessage = "Payment date is required")]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Payment mode is required")]
        [Display(Name = "Payment Mode")]
        public PaymentMode? PaymentMode { get; set; }  // ✅ Changed to nullable

        [Required(ErrorMessage = "Amount paid is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999.99")]
        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }

        [StringLength(50, ErrorMessage = "Transaction reference cannot exceed 50 characters")]
        [Display(Name = "Transaction Reference")]
        public string? TransactionReference { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
    }

    public class PaymentUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Payment date is required")]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Payment mode is required")]
        [Display(Name = "Payment Mode")]
        public PaymentMode PaymentMode { get; set; }

        [Required(ErrorMessage = "Amount paid is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999.99")]
        [Display(Name = "Amount Paid")]
        public decimal AmountPaid { get; set; }

        [StringLength(50, ErrorMessage = "Transaction reference cannot exceed 50 characters")]
        [Display(Name = "Transaction Reference")]
        public string? TransactionReference { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
    }
    public class PaymentSearchViewModel
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }
        [Display(Name = "Invoice ID")]
        public int? InvoiceId { get; set; }
        [Display(Name = "Payment Mode")]
        public PaymentMode? PaymentMode { get; set; }
        [Display(Name = "Date From")]
        [DataType(DataType.Date)]
        public DateTime? DateFrom { get; set; }
        [Display(Name = "Date To")]
        [DataType(DataType.Date)]
        public DateTime? DateTo { get; set; }
        [Display(Name = "Min Amount")]
        public decimal? MinAmount { get; set; }
        [Display(Name = "Max Amount")]
        public decimal? MaxAmount { get; set; }
    }

    public class PaymentStatisticsViewModel
    {
        [Display(Name = "Total Payments")]
        public int TotalPayments { get; set; }
        [Display(Name = "Total Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalAmount { get; set; }
        [Display(Name = "Average Payment")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AveragePayment { get; set; }
        [Display(Name = "Today's Payments")]
        public int TodayPayments { get; set; }
        [Display(Name = "Today's Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TodayAmount { get; set; }
        public Dictionary<string, decimal> PaymentModeDistribution { get; set; } = new();
    }
}