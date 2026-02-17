namespace InventoryPro.Domain.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    Guid? TenantId { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
}
