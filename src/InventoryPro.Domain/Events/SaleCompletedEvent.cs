using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Events;

public class SaleCompletedEvent : DomainEvent
{
    public Guid TenantId { get; }
    public Guid SaleId { get; }
    public string InvoiceNumber { get; }
    public Guid? CustomerId { get; }
    public decimal TotalAmount { get; }
    public int ItemCount { get; }
    public Guid CashierId { get; }
    public int LoyaltyPointsEarned { get; }

    public SaleCompletedEvent(
        Guid tenantId,
        Guid saleId,
        string invoiceNumber,
        Guid? customerId,
        decimal totalAmount,
        int itemCount,
        Guid cashierId,
        int loyaltyPointsEarned)
    {
        TenantId = tenantId;
        SaleId = saleId;
        InvoiceNumber = invoiceNumber;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        ItemCount = itemCount;
        CashierId = cashierId;
        LoyaltyPointsEarned = loyaltyPointsEarned;
    }
}
