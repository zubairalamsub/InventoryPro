using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public Guid? BatchLotId { get; set; }

    public decimal Profit => (UnitPrice - CostPrice) * Quantity - DiscountAmount;

    // Navigation properties
    public virtual Sale Sale { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual ProductVariant? ProductVariant { get; set; }
    public virtual BatchLot? BatchLot { get; set; }
    public virtual ICollection<SaleReturnItem> ReturnItems { get; set; } = new List<SaleReturnItem>();
}
