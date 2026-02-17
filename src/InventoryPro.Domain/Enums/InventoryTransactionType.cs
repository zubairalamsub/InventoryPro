namespace InventoryPro.Domain.Enums;

public enum InventoryTransactionType
{
    StockIn = 0,
    StockOut = 1,
    Adjustment = 2,
    Transfer = 3,
    Return = 4,
    Damage = 5,
    Expired = 6,
    Sale = 7,
    Purchase = 8,
    InitialStock = 9
}
