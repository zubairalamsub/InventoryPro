using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class LoyaltyTransaction : BaseEntity
{
    public Guid CustomerId { get; set; }
    public int Points { get; set; }
    public string Type { get; set; } = string.Empty;
    public Guid? ReferenceId { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
}
