using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace InventoryPro.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiration,
    DateTime RefreshTokenExpiration,
    UserInfo User);

public record UserInfo(
    Guid Id,
    string Email,
    string FullName,
    Guid TenantId,
    IEnumerable<string> Roles);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}

public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTime _dateTime;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        IRepository<RefreshToken> refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IDateTime dateTime)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("Invalid email or password."));
        }

        if (!user.IsActive)
        {
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("Your account has been deactivated."));
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("Your account has been locked due to too many failed login attempts."));
        }

        if (!result.Succeeded)
        {
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("Invalid email or password."));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles, user.TenantId);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        user.LastLoginAt = _dateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Result.Success(new LoginResponse(
            accessToken,
            refreshToken.Token,
            _dateTime.UtcNow.AddMinutes(15),
            refreshToken.ExpiresAt,
            new UserInfo(
                user.Id,
                user.Email!,
                user.FullName,
                user.TenantId,
                roles)));
    }
}
