using InventoryPro.Domain.Common;
using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class Discount : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DiscountType Type { get; set; }
    public decimal Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public bool ApplicableToAll { get; set; } = true;
    public Guid[]? CategoryIds { get; set; }
    public Guid[]? ProductIds { get; set; }
    public Guid[]? CustomerGroupIds { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public bool IsValid => IsActive &&
        (!StartDate.HasValue || StartDate.Value <= DateTime.UtcNow) &&
        (!EndDate.HasValue || EndDate.Value >= DateTime.UtcNow);

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
}
