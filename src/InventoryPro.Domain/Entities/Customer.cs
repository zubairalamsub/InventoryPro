using InventoryPro.Domain.Common;
using InventoryPro.Domain.Common.ValueObjects;

namespace InventoryPro.Domain.Entities;

public class Customer : BaseAuditableEntity, ITenantEntity, IAggregateRoot
{
    public Guid TenantId { get; set; }
    public Guid? CustomerGroupId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public Address? Address { get; set; }
    public string? TaxIdentificationNo { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public int LoyaltyPoints { get; set; }
    public decimal TotalPurchases { get; set; }
    public int TotalOrders { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Notes { get; set; }
    public string[]? Tags { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual CustomerGroup? CustomerGroup { get; set; }
    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public virtual ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<LoyaltyTransaction>();
    public virtual ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}
