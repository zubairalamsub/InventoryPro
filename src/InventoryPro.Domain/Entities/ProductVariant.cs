using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public decimal AdditionalCost { get; set; }
    public decimal AdditionalPrice { get; set; }
    public string Attributes { get; set; } = "{}";
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<StockLevel> StockLevels { get; set; } = new List<StockLevel>();
}
