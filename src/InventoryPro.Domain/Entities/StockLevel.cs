using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class StockLevel : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid? WarehouseZoneId { get; set; }
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public DateTime? LastStockTakeDate { get; set; }
    public DateTime LastUpdated { get; set; }

    public int AvailableQuantity => Quantity - ReservedQuantity;

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual ProductVariant? ProductVariant { get; set; }
    public virtual Warehouse Warehouse { get; set; } = null!;
    public virtual WarehouseZone? WarehouseZone { get; set; }
}
