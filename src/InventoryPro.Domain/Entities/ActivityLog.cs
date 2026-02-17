using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class ActivityLog : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public string Activity { get; set; } = string.Empty;
    public string? Details { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
}
