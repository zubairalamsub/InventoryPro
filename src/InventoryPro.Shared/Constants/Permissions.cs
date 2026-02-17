namespace InventoryPro.Shared.Constants;

public static class Permissions
{
    public static class Products
    {
        public const string View = "Permissions.Products.View";
        public const string Create = "Permissions.Products.Create";
        public const string Edit = "Permissions.Products.Edit";
        public const string Delete = "Permissions.Products.Delete";
        public const string Import = "Permissions.Products.Import";
        public const string Export = "Permissions.Products.Export";
    }

    public static class Categories
    {
        public const string View = "Permissions.Categories.View";
        public const string Create = "Permissions.Categories.Create";
        public const string Edit = "Permissions.Categories.Edit";
        public const string Delete = "Permissions.Categories.Delete";
    }

    public static class Inventory
    {
        public const string View = "Permissions.Inventory.View";
        public const string StockIn = "Permissions.Inventory.StockIn";
        public const string StockOut = "Permissions.Inventory.StockOut";
        public const string Adjust = "Permissions.Inventory.Adjust";
        public const string Transfer = "Permissions.Inventory.Transfer";
        public const string StockTake = "Permissions.Inventory.StockTake";
    }

    public static class Warehouses
    {
        public const string View = "Permissions.Warehouses.View";
        public const string Create = "Permissions.Warehouses.Create";
        public const string Edit = "Permissions.Warehouses.Edit";
        public const string Delete = "Permissions.Warehouses.Delete";
    }

    public static class Suppliers
    {
        public const string View = "Permissions.Suppliers.View";
        public const string Create = "Permissions.Suppliers.Create";
        public const string Edit = "Permissions.Suppliers.Edit";
        public const string Delete = "Permissions.Suppliers.Delete";
    }

    public static class PurchaseOrders
    {
        public const string View = "Permissions.PurchaseOrders.View";
        public const string Create = "Permissions.PurchaseOrders.Create";
        public const string Edit = "Permissions.PurchaseOrders.Edit";
        public const string Delete = "Permissions.PurchaseOrders.Delete";
        public const string Approve = "Permissions.PurchaseOrders.Approve";
        public const string Receive = "Permissions.PurchaseOrders.Receive";
    }

    public static class Sales
    {
        public const string View = "Permissions.Sales.View";
        public const string Create = "Permissions.Sales.Create";
        public const string Void = "Permissions.Sales.Void";
        public const string Return = "Permissions.Sales.Return";
        public const string ApplyDiscount = "Permissions.Sales.ApplyDiscount";
    }

    public static class Customers
    {
        public const string View = "Permissions.Customers.View";
        public const string Create = "Permissions.Customers.Create";
        public const string Edit = "Permissions.Customers.Edit";
        public const string Delete = "Permissions.Customers.Delete";
    }

    public static class Reports
    {
        public const string ViewSales = "Permissions.Reports.ViewSales";
        public const string ViewInventory = "Permissions.Reports.ViewInventory";
        public const string ViewPurchases = "Permissions.Reports.ViewPurchases";
        public const string ViewFinancial = "Permissions.Reports.ViewFinancial";
        public const string Export = "Permissions.Reports.Export";
    }

    public static class Settings
    {
        public const string View = "Permissions.Settings.View";
        public const string Edit = "Permissions.Settings.Edit";
        public const string ManageUsers = "Permissions.Settings.ManageUsers";
        public const string ManageRoles = "Permissions.Settings.ManageRoles";
        public const string ViewAuditLogs = "Permissions.Settings.ViewAuditLogs";
    }

    public static class Subscriptions
    {
        public const string View = "Permissions.Subscriptions.View";
        public const string Manage = "Permissions.Subscriptions.Manage";
    }
}
