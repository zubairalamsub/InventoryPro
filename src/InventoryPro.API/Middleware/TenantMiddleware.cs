using InventoryPro.Domain.Interfaces;

namespace InventoryPro.API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
    {
        var tenantId = GetTenantId(context);

        if (tenantId.HasValue)
        {
            tenantProvider.SetTenantId(tenantId.Value);
            _logger.LogDebug("Tenant context set: {TenantId}", tenantId.Value);
        }

        await _next(context);
    }

    private static Guid? GetTenantId(HttpContext context)
    {
        // First, try to get from claims (JWT)
        var tenantClaim = context.User.FindFirst("tenant_id");
        if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var claimTenantId))
        {
            return claimTenantId;
        }

        // Then, try from header
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue) &&
            Guid.TryParse(headerValue.FirstOrDefault(), out var headerTenantId))
        {
            return headerTenantId;
        }

        return null;
    }
}
