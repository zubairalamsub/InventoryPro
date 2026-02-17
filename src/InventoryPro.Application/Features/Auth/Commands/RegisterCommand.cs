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
    string? CompanyName = null,
    Guid? TenantId = null) : ICommand<RegisterResponse>;

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

        // Either TenantId or CompanyName must be provided
        RuleFor(x => x)
            .Must(x => x.TenantId.HasValue || !string.IsNullOrWhiteSpace(x.CompanyName))
            .WithMessage("Either TenantId or CompanyName is required");
    }
}

public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponse>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IRepository<Tenant> tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
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

        Guid tenantId;

        if (request.TenantId.HasValue)
        {
            // Validate existing tenant
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId.Value, cancellationToken);
            if (tenant == null || !tenant.IsActive)
            {
                return Result.Failure<RegisterResponse>(
                    Error.Validation("Auth.InvalidTenant", "The specified tenant is invalid or inactive."));
            }
            tenantId = tenant.Id;
        }
        else
        {
            // Create new tenant with CompanyName
            var subdomain = request.CompanyName!.ToLower()
                .Replace(" ", "-")
                .Replace(".", "")
                .Replace("'", "");

            var newTenant = new Tenant
            {
                Name = request.CompanyName!,
                Subdomain = subdomain + "-" + Guid.NewGuid().ToString()[..8],
                IsActive = true
            };

            await _tenantRepository.AddAsync(newTenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            tenantId = newTenant.Id;
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            TenantId = tenantId,
            Role = request.TenantId.HasValue ? Domain.Enums.UserRole.Viewer : Domain.Enums.UserRole.TenantAdmin,
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

        // Add to TenantAdmin role if creating new company
        if (!request.TenantId.HasValue)
        {
            if (!await _roleManager.RoleExistsAsync("TenantAdmin"))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>("TenantAdmin"));
            }
            await _userManager.AddToRoleAsync(user, "TenantAdmin");
        }

        return Result.Success(new RegisterResponse(user.Id, user.Email!));
    }
}
