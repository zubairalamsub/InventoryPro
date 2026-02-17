using Hangfire.Dashboard;
using InventoryPro.Shared.Constants;

namespace InventoryPro.API.Middleware;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // In development, allow access without authentication
        var environment = httpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
        if (environment.IsDevelopment())
        {
            return true;
        }

        // In production, require SystemAdmin role
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.IsInRole(Roles.SuperAdmin);
    }
}
