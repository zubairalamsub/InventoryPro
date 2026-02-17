using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class StockAdjustmentItem : BaseEntity
{
    public Guid StockAdjustmentId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public int QuantityAdjusted { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual StockAdjustment StockAdjustment { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual ProductVariant? ProductVariant { get; set; }
}
