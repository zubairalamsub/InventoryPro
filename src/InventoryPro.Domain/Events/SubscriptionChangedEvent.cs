using InventoryPro.Domain.Common;
using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Events;

public class SubscriptionChangedEvent : DomainEvent
{
    public Guid TenantId { get; }
    public Guid SubscriptionId { get; }
    public SubscriptionPlanType PreviousPlan { get; }
    public SubscriptionPlanType NewPlan { get; }
    public SubscriptionStatus Status { get; }
    public string ChangeType { get; }

    public SubscriptionChangedEvent(
        Guid tenantId,
        Guid subscriptionId,
        SubscriptionPlanType previousPlan,
        SubscriptionPlanType newPlan,
        SubscriptionStatus status,
        string changeType)
    {
        TenantId = tenantId;
        SubscriptionId = subscriptionId;
        PreviousPlan = previousPlan;
        NewPlan = newPlan;
        Status = status;
        ChangeType = changeType;
    }
}
