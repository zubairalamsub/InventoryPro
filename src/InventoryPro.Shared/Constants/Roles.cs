namespace InventoryPro.Shared.Constants;

public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string TenantAdmin = "TenantAdmin";
    public const string Manager = "Manager";
    public const string Cashier = "Cashier";
    public const string Viewer = "Viewer";
    public const string Accountant = "Accountant";

    public static readonly string[] All = { SuperAdmin, TenantAdmin, Manager, Cashier, Viewer, Accountant };

    public static readonly string[] AdminRoles = { SuperAdmin, TenantAdmin };

    public static readonly string[] ManagementRoles = { SuperAdmin, TenantAdmin, Manager };
}
