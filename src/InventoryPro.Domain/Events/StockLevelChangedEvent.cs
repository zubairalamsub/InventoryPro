using InventoryPro.Domain.Common;
using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Events;

public class StockLevelChangedEvent : DomainEvent
{
    public Guid TenantId { get; }
    public Guid ProductId { get; }
    public Guid? ProductVariantId { get; }
    public Guid WarehouseId { get; }
    public int PreviousQuantity { get; }
    public int NewQuantity { get; }
    public int QuantityChanged { get; }
    public InventoryTransactionType TransactionType { get; }
    public Guid? ReferenceId { get; }

    public StockLevelChangedEvent(
        Guid tenantId,
        Guid productId,
        Guid? productVariantId,
        Guid warehouseId,
        int previousQuantity,
        int newQuantity,
        InventoryTransactionType transactionType,
        Guid? referenceId = null)
    {
        TenantId = tenantId;
        ProductId = productId;
        ProductVariantId = productVariantId;
        WarehouseId = warehouseId;
        PreviousQuantity = previousQuantity;
        NewQuantity = newQuantity;
        QuantityChanged = newQuantity - previousQuantity;
        TransactionType = transactionType;
        ReferenceId = referenceId;
    }
}
