using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class BatchLot : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid ProductId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string? LotNumber { get; set; }
    public DateOnly? ManufactureDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public int RemainingQuantity { get; set; }
    public decimal? CostPrice { get; set; }
    public string? Notes { get; set; }

    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsExpiringSoon(int days = 30) => ExpiryDate.HasValue &&
        ExpiryDate.Value <= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}
