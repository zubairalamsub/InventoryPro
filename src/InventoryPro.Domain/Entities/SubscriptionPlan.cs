using InventoryPro.Domain.Common;
using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public SubscriptionPlanType Type { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public int? MaxProducts { get; set; }
    public int? MaxUsers { get; set; }
    public int? MaxWarehouses { get; set; }
    public int? MaxTransactionsPerMonth { get; set; }
    public string Features { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    // Navigation properties
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
