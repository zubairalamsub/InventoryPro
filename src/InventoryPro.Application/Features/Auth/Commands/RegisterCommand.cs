using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace InventoryPro.Application.Features.Auth.Commands;

public record RegisterCommand(
    string Email,
    string Password,
    string FullName,
    Guid TenantId) : ICommand<RegisterResponse>;

public record RegisterResponse(Guid UserId, string Email);

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters");

        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("Tenant ID is required");
    }
}

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IRepository<Tenant> _tenantRepository;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IRepository<Tenant> tenantRepository)
    {
        _userManager = userManager;
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Result.Failure<RegisterResponse>(
                Error.Conflict("Auth.EmailExists", "A user with this email already exists."));
        }

        // Validate tenant
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null || !tenant.IsActive)
        {
            return Result.Failure<RegisterResponse>(
                Error.Validation("Auth.InvalidTenant", "The specified tenant is invalid or inactive."));
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            TenantId = request.TenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure<RegisterResponse>(
                Error.Validation("Auth.RegistrationFailed", errors));
        }

        return Result.Success(new RegisterResponse(user.Id, user.Email!));
    }
}
