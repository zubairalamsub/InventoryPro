using InventoryPro.Domain.Common;
using InventoryPro.Domain.Common.ValueObjects;

namespace InventoryPro.Domain.Entities;

public class Supplier : BaseAuditableEntity, ITenantEntity, IAggregateRoot
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public string? TaxIdentificationNo { get; set; }
    public string? Website { get; set; }
    public Address? Address { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public string? BankDetails { get; set; }
    public decimal? Rating { get; set; }
    public string? Notes { get; set; }
    public string[]? Tags { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<SupplierProduct> SupplierProducts { get; set; } = new List<SupplierProduct>();
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}
