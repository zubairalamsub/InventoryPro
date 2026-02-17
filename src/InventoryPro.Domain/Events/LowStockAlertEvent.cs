using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Events;

public class LowStockAlertEvent : DomainEvent
{
    public Guid TenantId { get; }
    public Guid ProductId { get; }
    public string ProductName { get; }
    public string ProductSku { get; }
    public Guid WarehouseId { get; }
    public string WarehouseName { get; }
    public int CurrentQuantity { get; }
    public int ReorderLevel { get; }

    public LowStockAlertEvent(
        Guid tenantId,
        Guid productId,
        string productName,
        string productSku,
        Guid warehouseId,
        string warehouseName,
        int currentQuantity,
        int reorderLevel)
    {
        TenantId = tenantId;
        ProductId = productId;
        ProductName = productName;
        ProductSku = productSku;
        WarehouseId = warehouseId;
        WarehouseName = warehouseName;
        CurrentQuantity = currentQuantity;
        ReorderLevel = reorderLevel;
    }
}
