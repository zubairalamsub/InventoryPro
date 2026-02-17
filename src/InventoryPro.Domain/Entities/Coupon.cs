using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class Coupon : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid DiscountId { get; set; }
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }
    public int MaxUsesPerCustomer { get; set; } = 1;
    public bool IsActive { get; set; } = true;

    public bool IsValid => IsActive && (!MaxUses.HasValue || UsedCount < MaxUses);

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Discount Discount { get; set; } = null!;
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
