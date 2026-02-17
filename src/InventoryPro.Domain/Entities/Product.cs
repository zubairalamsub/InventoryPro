using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class Product : BaseAuditableEntity, ITenantEntity, IAggregateRoot
{
    public Guid TenantId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? UnitOfMeasureId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? MinimumPrice { get; set; }
    public Guid? TaxConfigurationId { get; set; }
    public int ReorderLevel { get; set; }
    public int ReorderQuantity { get; set; }
    public int? MaxStockLevel { get; set; }
    public decimal? Weight { get; set; }
    public string? Dimensions { get; set; }
    public string[]? Tags { get; set; }
    public string? CustomFields { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsService { get; set; }
    public bool TrackInventory { get; set; } = true;
    public bool AllowNegativeStock { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Category? Category { get; set; }
    public virtual UnitOfMeasure? UnitOfMeasure { get; set; }
    public virtual TaxConfiguration? TaxConfiguration { get; set; }
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
    public virtual ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();
    public virtual ICollection<BatchLot> BatchLots { get; set; } = new List<BatchLot>();
}
