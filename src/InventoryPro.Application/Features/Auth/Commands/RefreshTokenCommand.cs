using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace InventoryPro.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : ICommand<RefreshTokenResponse>;

public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiration,
    DateTime RefreshTokenExpiration);

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTime _dateTime;

    public RefreshTokenCommandHandler(
        IRepository<RefreshToken> refreshTokenRepository,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        IDateTime dateTime)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _refreshTokenRepository.FirstOrDefaultAsync(
            t => t.Token == request.RefreshToken, cancellationToken);

        if (storedToken == null)
        {
            return Result.Failure<RefreshTokenResponse>(
                Error.Unauthorized("Invalid refresh token."));
        }

        if (storedToken.IsRevoked)
        {
            return Result.Failure<RefreshTokenResponse>(
                Error.Unauthorized("Refresh token has been revoked."));
        }

        if (storedToken.ExpiresAt < _dateTime.UtcNow)
        {
            return Result.Failure<RefreshTokenResponse>(
                Error.Unauthorized("Refresh token has expired."));
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user == null || !user.IsActive)
        {
            return Result.Failure<RefreshTokenResponse>(
                Error.Unauthorized("User not found or inactive."));
        }

        // Revoke old refresh token
        storedToken.RevokedAt = _dateTime.UtcNow;
        _refreshTokenRepository.Update(storedToken);

        // Generate new tokens
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles, user.TenantId);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

        // Store new refresh token
        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RefreshTokenResponse(
            accessToken,
            newRefreshToken.Token,
            _dateTime.UtcNow.AddMinutes(15),
            newRefreshToken.ExpiresAt));
    }
}
