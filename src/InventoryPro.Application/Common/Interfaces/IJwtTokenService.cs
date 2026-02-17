using InventoryPro.Domain.Entities;

namespace InventoryPro.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(ApplicationUser user, IEnumerable<string> roles, Guid? tenantId = null);
    RefreshToken GenerateRefreshToken(Guid userId);
    bool ValidateToken(string token);
}
