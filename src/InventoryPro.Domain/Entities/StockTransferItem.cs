using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class StockTransferItem : BaseEntity
{
    public Guid StockTransferId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public int? ReceivedQuantity { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public virtual StockTransfer StockTransfer { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual ProductVariant? ProductVariant { get; set; }
}
