using InventoryPro.Domain.Common;

namespace InventoryPro.Domain.Entities;

public class InvoiceSetting : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; set; }
    public string Prefix { get; set; } = "INV-";
    public int NextNumber { get; set; } = 1;
    public string Template { get; set; } = "default";
    public bool ShowLogo { get; set; } = true;
    public bool ShowTaxBreakdown { get; set; } = true;
    public string? FooterText { get; set; }
    public string? TermsAndConditions { get; set; }
    public string? BankDetails { get; set; }
    public string? CustomCss { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;

    public string GenerateInvoiceNumber()
    {
        var number = $"{Prefix}{NextNumber:D6}";
        NextNumber++;
        return number;
    }
}
