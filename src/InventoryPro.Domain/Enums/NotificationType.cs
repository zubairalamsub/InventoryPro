namespace InventoryPro.Domain.Enums;

public enum NotificationType
{
    LowStock = 0,
    OrderReceived = 1,
    SaleCompleted = 2,
    System = 3,
    PurchaseOrderApproved = 4,
    PurchaseOrderRejected = 5,
    NewUserInvite = 6,
    PasswordReset = 7,
    SubscriptionExpiring = 8,
    PaymentFailed = 9,
    ExpiringBatch = 10,
    StockTransferPending = 11,
    QuotationExpiring = 12
}
