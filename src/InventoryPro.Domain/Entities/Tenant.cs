using InventoryPro.Domain.Common;
using InventoryPro.Domain.Common.ValueObjects;
using InventoryPro.Domain.Enums;

namespace InventoryPro.Domain.Entities;

public class Tenant : BaseEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string? CustomDomain { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string PrimaryColor { get; set; } = "#1976D2";
    public string SecondaryColor { get; set; } = "#424242";
    public string? BusinessType { get; set; }
    public string? BusinessRegistrationNo { get; set; }
    public string? TaxIdentificationNo { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public Address? Address { get; set; }
    public Guid? DefaultCurrencyId { get; set; }
    public Guid? DefaultWarehouseId { get; set; }
    public ValuationMethod ValuationMethod { get; set; } = ValuationMethod.WeightedAverage;
    public int FinancialYearStart { get; set; } = 1;
    public string TimeZone { get; set; } = "UTC";
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Currency? DefaultCurrency { get; set; }
    public virtual Warehouse? DefaultWarehouse { get; set; }
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public virtual ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
}
