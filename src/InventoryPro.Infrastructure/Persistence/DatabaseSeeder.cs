using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryPro.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedRolesAsync();
            await SeedCurrenciesAsync();
            await SeedSubscriptionPlansAsync();
            await SeedDefaultTenantAsync();
            await SeedSystemAdminAsync();

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in Roles.All)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
                _logger.LogInformation("Created role: {Role}", role);
            }
        }
    }

    private async Task SeedCurrenciesAsync()
    {
        if (await _context.Currencies.AnyAsync())
            return;

        var currencies = new List<Currency>
        {
            new() { Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2, IsActive = true },
            new() { Code = "EUR", Name = "Euro", Symbol = "€", DecimalPlaces = 2, IsActive = true },
            new() { Code = "GBP", Name = "British Pound", Symbol = "£", DecimalPlaces = 2, IsActive = true },
            new() { Code = "JPY", Name = "Japanese Yen", Symbol = "¥", DecimalPlaces = 0, IsActive = true },
            new() { Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", DecimalPlaces = 2, IsActive = true },
            new() { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", DecimalPlaces = 2, IsActive = true },
            new() { Code = "CHF", Name = "Swiss Franc", Symbol = "CHF", DecimalPlaces = 2, IsActive = true },
            new() { Code = "CNY", Name = "Chinese Yuan", Symbol = "¥", DecimalPlaces = 2, IsActive = true },
            new() { Code = "INR", Name = "Indian Rupee", Symbol = "₹", DecimalPlaces = 2, IsActive = true },
            new() { Code = "MXN", Name = "Mexican Peso", Symbol = "$", DecimalPlaces = 2, IsActive = true },
            new() { Code = "BRL", Name = "Brazilian Real", Symbol = "R$", DecimalPlaces = 2, IsActive = true },
            new() { Code = "KRW", Name = "South Korean Won", Symbol = "₩", DecimalPlaces = 0, IsActive = true }
        };

        await _context.Currencies.AddRangeAsync(currencies);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} currencies", currencies.Count);
    }

    private async Task SeedSubscriptionPlansAsync()
    {
        if (await _context.SubscriptionPlans.AnyAsync())
            return;

        var plans = new List<SubscriptionPlan>
        {
            new()
            {
                Name = "Free",
                Type = SubscriptionPlanType.Free,
                MonthlyPrice = 0,
                AnnualPrice = 0,
                MaxUsers = 2,
                MaxProducts = 100,
                MaxWarehouses = 1,
                MaxTransactionsPerMonth = 500,
                Features = "{\"basicInventory\":true,\"singleWarehouse\":true,\"emailSupport\":true}",
                IsActive = true,
                SortOrder = 1
            },
            new()
            {
                Name = "Starter",
                Type = SubscriptionPlanType.Starter,
                MonthlyPrice = 29.99m,
                AnnualPrice = 299.99m,
                MaxUsers = 5,
                MaxProducts = 1000,
                MaxWarehouses = 2,
                MaxTransactionsPerMonth = 5000,
                Features = "{\"basicInventory\":true,\"multiWarehouse\":true,\"integrations\":true,\"prioritySupport\":true}",
                IsActive = true,
                SortOrder = 2
            },
            new()
            {
                Name = "Professional",
                Type = SubscriptionPlanType.Professional,
                MonthlyPrice = 79.99m,
                AnnualPrice = 799.99m,
                MaxUsers = 15,
                MaxProducts = 10000,
                MaxWarehouses = 5,
                MaxTransactionsPerMonth = 25000,
                Features = "{\"advancedAnalytics\":true,\"apiAccess\":true,\"phoneSupport\":true,\"batchTracking\":true}",
                IsActive = true,
                SortOrder = 3
            },
            new()
            {
                Name = "Enterprise",
                Type = SubscriptionPlanType.Enterprise,
                MonthlyPrice = 199.99m,
                AnnualPrice = 1999.99m,
                MaxUsers = null,
                MaxProducts = null,
                MaxWarehouses = null,
                MaxTransactionsPerMonth = null,
                Features = "{\"unlimited\":true,\"dedicatedSupport\":true,\"customIntegrations\":true,\"sla\":true}",
                IsActive = true,
                SortOrder = 4
            }
        };

        await _context.SubscriptionPlans.AddRangeAsync(plans);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} subscription plans", plans.Count);
    }

    private async Task SeedDefaultTenantAsync()
    {
        if (await _context.Tenants.AnyAsync(t => t.Subdomain == "demo"))
            return;

        var freePlan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Type == SubscriptionPlanType.Free);

        if (freePlan == null)
        {
            _logger.LogWarning("Free plan not found. Skipping default tenant seeding.");
            return;
        }

        var usdCurrency = await _context.Currencies
            .FirstOrDefaultAsync(c => c.Code == "USD");

        var tenant = new Tenant
        {
            Name = "Demo Company",
            Subdomain = "demo",
            IsActive = true,
            DefaultCurrencyId = usdCurrency?.Id
        };

        await _context.Tenants.AddAsync(tenant);
        await _context.SaveChangesAsync();

        // Create subscription for tenant
        var subscription = new Subscription
        {
            TenantId = tenant.Id,
            PlanId = freePlan.Id,
            Status = SubscriptionStatus.Active,
            CurrentPeriodStart = DateTime.UtcNow,
            CurrentPeriodEnd = DateTime.UtcNow.AddYears(100)
        };

        await _context.Subscriptions.AddAsync(subscription);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded default demo tenant");
    }

    private async Task SeedSystemAdminAsync()
    {
        var adminEmail = "admin@inventorypro.com";

        if (await _userManager.FindByEmailAsync(adminEmail) != null)
            return;

        var demoTenant = await _context.Tenants.FirstOrDefaultAsync(t => t.Subdomain == "demo");
        if (demoTenant == null)
        {
            _logger.LogWarning("Demo tenant not found. Skipping system admin seeding.");
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Administrator",
            TenantId = demoTenant.Id,
            Role = UserRole.SuperAdmin,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(admin, "Admin123!");

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(admin, Roles.SuperAdmin);
            _logger.LogInformation("Created system admin user: {Email}", adminEmail);
        }
        else
        {
            _logger.LogError("Failed to create system admin: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
