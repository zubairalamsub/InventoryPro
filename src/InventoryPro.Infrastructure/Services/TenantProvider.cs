using InventoryPro.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace InventoryPro.Infrastructure.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private Guid? _tenantId;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetTenantId()
    {
        if (_tenantId.HasValue)
            return _tenantId;

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            return null;

        // Try to get tenant ID from claims
        var tenantClaim = httpContext.User.FindFirst("tenant_id");
        if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tenantId))
        {
            _tenantId = tenantId;
            return _tenantId;
        }

        // Try to get from header (for API key auth scenarios)
        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue) &&
            Guid.TryParse(headerValue.FirstOrDefault(), out var headerTenantId))
        {
            _tenantId = headerTenantId;
            return _tenantId;
        }

        return null;
    }

    public void SetTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
    }
}
