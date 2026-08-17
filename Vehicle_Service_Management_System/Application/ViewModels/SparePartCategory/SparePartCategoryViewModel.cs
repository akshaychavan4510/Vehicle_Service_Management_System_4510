using System.ComponentModel.DataAnnotations;

namespace Vehicle_Service_Management_System.Application.ViewModels.SparePartCategory
{
    // ============================================================
    // List ViewModel – used for displaying categories in a grid/table
    // ============================================================
    public class SparePartCategoryListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        [Display(Name = "Status Badge")]
        public string StatusBadge => IsActive ? "bg-success" : "bg-danger"; // Green = Active, Red = Inactive

        [Display(Name = "Total Parts")]
        public int SparePartCount { get; set; }

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime CreatedOn { get; set; }
    }

    // ============================================================
    // Form ViewModel – used for Create/Edit (common form)
    // ============================================================
    public class SparePartCategoryFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true; // Default: active
    }

    // ============================================================
    // Create ViewModel – only for creation (no Id, no IsActive toggle)
    // ============================================================
    public class SparePartCategoryCreateViewModel
    {
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }

    // ============================================================
    // Update ViewModel – for editing, includes IsActive toggle
    // ============================================================
    public class SparePartCategoryUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }

    // ============================================================
    // Details ViewModel – full details including parts and statistics
    // ============================================================
    public class SparePartCategoryDetailsViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Status")]
        public bool IsActive { get; set; }

        [Display(Name = "Status Badge")]
        public string StatusBadge => IsActive ? "bg-success" : "bg-danger";

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime CreatedOn { get; set; }

        [Display(Name = "Modified Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
        public DateTime? ModifiedOn { get; set; }

        [Display(Name = "Total Parts")]
        public int TotalParts { get; set; }

        [Display(Name = "Total Stock Value")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalStockValue { get; set; }

        [Display(Name = "Average Part Price")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal AveragePartPrice { get; set; }

        public List<SparePartSummaryViewModel> SpareParts { get; set; } = new();
    }

    // ============================================================
    // Spare Part Summary – used inside Details ViewModel
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

        [Display(Name = "Status")]
        public bool IsActive { get; set; }
    }

    // ============================================================
    // Search / Filter ViewModel
    // ============================================================
    public class SparePartCategorySearchViewModel
    {
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }

        [Display(Name = "Active")]
        public bool? IsActive { get; set; } // Null = all, true/false to filter

        [Display(Name = "Has Parts")]
        public bool? HasParts { get; set; }
    }

    // ============================================================
    // Statistics / Dashboard ViewModel
    // ============================================================
    public class SparePartCategoryStatisticsViewModel
    {
        [Display(Name = "Total Categories")]
        public int TotalCategories { get; set; }

        [Display(Name = "Active Categories")]
        public int ActiveCategories { get; set; }

        [Display(Name = "Inactive Categories")]
        public int InactiveCategories { get; set; }

        [Display(Name = "Total Spare Parts")]
        public int TotalSpareParts { get; set; }

        [Display(Name = "Total Stock Value")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalStockValue { get; set; }

        [Display(Name = "Category with Most Parts")]
        public string? CategoryWithMostParts { get; set; }

        [Display(Name = "Category with Highest Stock Value")]
        public string? CategoryWithHighestStockValue { get; set; }

        [Display(Name = "Average Parts per Category")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AveragePartsPerCategory { get; set; }

        public Dictionary<string, int> PartsPerCategory { get; set; } = new();
    }
}