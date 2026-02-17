using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class HeldTransaction : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? ReferenceNote { get; set; }
    public string ItemsJson { get; set; } = "[]";
    public decimal SubTotal { get; set; }
    public Guid CashierId { get; set; }
    public DateTime HeldAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Customer? Customer { get; set; }
    public virtual ApplicationUser Cashier { get; set; } = null!;
}
