using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class SystemSetting : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string DataType { get; set; } = "string";

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
}
