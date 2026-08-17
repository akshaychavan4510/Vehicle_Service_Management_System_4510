using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Vehicle_Service_Management_System.Application.ViewModels.SparePart
{
    // ============================================================
    //  LIST VIEW MODEL
    // ============================================================
    public class SparePartListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Part Name")]
        public string PartName { get; set; } = string.Empty;

        [Display(Name = "Part Code")]
        public string PartCode { get; set; } = string.Empty;

        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Minimum Stock")]
        public int MinimumStock { get; set; }

        [Display(Name = "Unit")]
        public string? Unit { get; set; }

        // ✅ Use IsActive for status (active/inactive)
        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        [Display(Name = "Status Badge")]
        public string StatusBadge => IsActive ? "bg-success" : "bg-danger";

        [Display(Name = "Stock Status")]
        public string StockStatus => StockQuantity <= MinimumStock ? "Low Stock" : "In Stock";

        [Display(Name = "Stock Status Badge")]
        public string StockStatusBadge => StockQuantity <= MinimumStock ? "bg-warning" : "bg-info";

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }
    }

    // ============================================================
    //  FORM VIEW MODEL (for Create/Edit)
    // ============================================================
    public class SparePartFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Part name is required")]
        [StringLength(100, ErrorMessage = "Part name cannot exceed 100 characters")]
        [Display(Name = "Part Name")]
        public string PartName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Part code is required")]
        [StringLength(50, ErrorMessage = "Part code cannot exceed 50 characters")]
        [Display(Name = "Part Code")]
        public string PartCode { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Brand cannot exceed 50 characters")]
        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, 1000000, ErrorMessage = "Unit price must be between 0.01 and 1,000,000")]
        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, 100000, ErrorMessage = "Stock quantity must be between 0 and 100,000")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Minimum stock is required")]
        [Range(0, 100000, ErrorMessage = "Minimum stock must be between 0 and 100,000")]
        [Display(Name = "Minimum Stock")]
        public int MinimumStock { get; set; }

        [StringLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
        [Display(Name = "Unit")]
        public string? Unit { get; set; }

        // ✅ Use IsActive for active/inactive (default: true)
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int SparePartCategoryId { get; set; }

        public List<SelectListItem> Categories { get; set; } = new();
    }

    // ============================================================
    //  DETAILS VIEW MODEL
    // ============================================================
    public class SparePartDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Part Name")]
        public string PartName { get; set; } = string.Empty;

        [Display(Name = "Part Code")]
        public string PartCode { get; set; } = string.Empty;

        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Minimum Stock")]
        public int MinimumStock { get; set; }

        [Display(Name = "Unit")]
        public string? Unit { get; set; }

        // ✅ Use IsActive for status
        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        [Display(Name = "Status Badge")]
        public string StatusBadge => IsActive ? "bg-success" : "bg-danger";

        [Display(Name = "Stock Status")]
        public string StockStatus => StockQuantity <= MinimumStock ? "Low Stock" : "In Stock";

        [Display(Name = "Stock Status Badge")]
        public string StockStatusBadge => StockQuantity <= MinimumStock ? "bg-warning" : "bg-info";

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Modified Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? ModifiedOn { get; set; }

        [Display(Name = "Total Used in Invoices")]
        public int TotalUsed { get; set; }

        [Display(Name = "Total Revenue")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalRevenue { get; set; }
    }

    // ============================================================
    //  CREATE VIEW MODEL
    // ============================================================
    public class SparePartCreateViewModel
    {
        [Required(ErrorMessage = "Part name is required")]
        [StringLength(100, ErrorMessage = "Part name cannot exceed 100 characters")]
        [Display(Name = "Part Name")]
        public string PartName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Part code is required")]
        [StringLength(50, ErrorMessage = "Part code cannot exceed 50 characters")]
        [Display(Name = "Part Code")]
        public string PartCode { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Brand cannot exceed 50 characters")]
        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, 1000000, ErrorMessage = "Unit price must be between 0.01 and 1,000,000")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, 100000, ErrorMessage = "Stock quantity must be between 0 and 100,000")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Minimum stock is required")]
        [Range(0, 100000, ErrorMessage = "Minimum stock must be between 0 and 100,000")]
        [Display(Name = "Minimum Stock")]
        public int MinimumStock { get; set; }

        [StringLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
        [Display(Name = "Unit")]
        public string? Unit { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int SparePartCategoryId { get; set; }
    }

    // ============================================================
    //  UPDATE VIEW MODEL
    // ============================================================
    public class SparePartUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Part name is required")]
        [StringLength(100, ErrorMessage = "Part name cannot exceed 100 characters")]
        [Display(Name = "Part Name")]
        public string PartName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Part code is required")]
        [StringLength(50, ErrorMessage = "Part code cannot exceed 50 characters")]
        [Display(Name = "Part Code")]
        public string PartCode { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Brand cannot exceed 50 characters")]
        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [Required(ErrorMessage = "Unit price is required")]
        [Range(0.01, 1000000, ErrorMessage = "Unit price must be between 0.01 and 1,000,000")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, 100000, ErrorMessage = "Stock quantity must be between 0 and 100,000")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Minimum stock is required")]
        [Range(0, 100000, ErrorMessage = "Minimum stock must be between 0 and 100,000")]
        [Display(Name = "Minimum Stock")]
        public int MinimumStock { get; set; }

        [StringLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
        [Display(Name = "Unit")]
        public string? Unit { get; set; }

        // ✅ Use IsActive for active/inactive
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int SparePartCategoryId { get; set; }
    }

    // ============================================================
    //  SEARCH VIEW MODEL
    // ============================================================
    public class SparePartSearchViewModel
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [Display(Name = "Min Price")]
        public decimal? MinPrice { get; set; }

        [Display(Name = "Max Price")]
        public decimal? MaxPrice { get; set; }

        [Display(Name = "Stock Status")]
        public string? StockStatus { get; set; } // "LowStock", "InStock"

        // ✅ Use IsActive to filter active/inactive
        [Display(Name = "Is Active")]
        public bool? IsActive { get; set; }

        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> StockStatuses { get; set; } = new()
        {
            new SelectListItem { Value = "LowStock", Text = "Low Stock" },
            new SelectListItem { Value = "InStock", Text = "In Stock" }
        };
    }

    // ============================================================
    //  STOCK UPDATE VIEW MODEL
    // ============================================================
    public class SparePartStockUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Display(Name = "Part Name")]
        public string PartName { get; set; } = string.Empty;

        [Display(Name = "Current Stock")]
        public int CurrentStock { get; set; }

        [Required(ErrorMessage = "New stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a positive number")]
        [Display(Name = "New Stock Quantity")]
        public int NewStockQuantity { get; set; }

        [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
        [Display(Name = "Reason for Update")]
        public string? Reason { get; set; }
    }

    // ============================================================
    //  STATISTICS VIEW MODEL
    // ============================================================
    public class SparePartStatisticsViewModel
    {
        [Display(Name = "Total Parts")]
        public int TotalParts { get; set; }

        [Display(Name = "Active Parts")]
        public int ActiveParts { get; set; }

        [Display(Name = "Inactive Parts")]
        public int InactiveParts { get; set; }

        [Display(Name = "Low Stock Items")]
        public int LowStockItems { get; set; }

        [Display(Name = "Total Stock Value")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalStockValue { get; set; }

        [Display(Name = "Average Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AveragePrice { get; set; }

        [Display(Name = "Most Expensive Part")]
        public string? MostExpensivePart { get; set; }

        [Display(Name = "Most Stocked Part")]
        public string? MostStockedPart { get; set; }

        [Display(Name = "Most Used Part")]
        public string? MostUsedPart { get; set; }

        public Dictionary<string, int> CategoryDistribution { get; set; } = new();
        public Dictionary<string, int> BrandDistribution { get; set; } = new();
    }

    // ============================================================
    //  LOW STOCK VIEW MODEL
    // ============================================================
    public class SparePartLowStockViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Part Name")]
        public string PartName { get; set; } = string.Empty;

        [Display(Name = "Part Code")]
        public string PartCode { get; set; } = string.Empty;

        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [Display(Name = "Category")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Current Stock")]
        public int StockQuantity { get; set; }

        [Display(Name = "Minimum Stock")]
        public int MinimumStock { get; set; }

        [Display(Name = "Shortage")]
        public int Shortage => MinimumStock - StockQuantity;

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal UnitPrice { get; set; }
    }

    // ============================================================
    //  SUMMARY VIEW MODEL (used in other views, e.g., Category Details)
    //  ✅ Note: This is the one in the SparePart namespace.
    //  For the SparePartCategory namespace, we have a separate one.
    // ============================================================
    public class SparePartSummaryViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Part Name")]
        public string PartName { get; set; } = string.Empty;

        [Display(Name = "Part Code")]
        public string PartCode { get; set; } = string.Empty;

        [Display(Name = "Brand")]
        public string? Brand { get; set; }

        [Display(Name = "Unit Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal UnitPrice { get; set; }

        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }

        [Display(Name = "Unit")]
        public string? Unit { get; set; }

        // ✅ Use IsActive for status
        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        // ❌ Do NOT include IsDeleted here – it's not needed for summaries.
    }
}