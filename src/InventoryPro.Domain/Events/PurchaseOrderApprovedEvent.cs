using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Events;

public class PurchaseOrderApprovedEvent : DomainEvent
{
    public Guid TenantId { get; }
    public Guid PurchaseOrderId { get; }
    public string OrderNumber { get; }
    public Guid SupplierId { get; }
    public string SupplierName { get; }
    public decimal TotalAmount { get; }
    public Guid ApprovedBy { get; }

    public PurchaseOrderApprovedEvent(
        Guid tenantId,
        Guid purchaseOrderId,
        string orderNumber,
        Guid supplierId,
        string supplierName,
        decimal totalAmount,
        Guid approvedBy)
    {
        TenantId = tenantId;
        PurchaseOrderId = purchaseOrderId;
        OrderNumber = orderNumber;
        SupplierId = supplierId;
        SupplierName = supplierName;
        TotalAmount = totalAmount;
        ApprovedBy = approvedBy;
    }
}
