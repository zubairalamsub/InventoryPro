using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Common.ValueObjects;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Customers.Commands;

public record CreateCustomerCommand(
    string Name,
    Guid? CustomerGroupId = null,
    string? Code = null,
    string? Email = null,
    string? Phone = null,
    string? AlternatePhone = null,
    string? Street = null,
    string? City = null,
    string? State = null,
    string? PostalCode = null,
    string? Country = null,
    string? TaxIdentificationNo = null,
    decimal? CreditLimit = null,
    DateOnly? DateOfBirth = null,
    string? Notes = null,
    string[]? Tags = null
) : ICommand<CustomerResponse>;

public record CustomerResponse(
    Guid Id,
    string Name,
    string? Code,
    string? Email,
    string? Phone,
    decimal CurrentBalance,
    int LoyaltyPoints,
    bool IsActive,
    DateTime CreatedAt);

public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200).WithMessage("Customer name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email address")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters")
            .When(x => !string.IsNullOrEmpty(x.Phone));
    }
}

public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, CustomerResponse>
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;

    public CreateCustomerCommandHandler(
        IRepository<Customer> customerRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
    }

    public async Task<Result<CustomerResponse>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (!tenantId.HasValue)
        {
            return Result.Failure<CustomerResponse>(
                Error.Unauthorized("Tenant context is required."));
        }

        // Check for duplicate code within tenant
        if (!string.IsNullOrEmpty(request.Code))
        {
            var existingCustomer = await _customerRepository.FirstOrDefaultAsync(
                c => c.Code == request.Code && c.TenantId == tenantId.Value,
                cancellationToken);

            if (existingCustomer != null)
            {
                return Result.Failure<CustomerResponse>(
                    Error.Conflict("Customer.DuplicateCode", $"A customer with code '{request.Code}' already exists."));
            }
        }

        // Check for duplicate email within tenant
        if (!string.IsNullOrEmpty(request.Email))
        {
            var existingCustomer = await _customerRepository.FirstOrDefaultAsync(
                c => c.Email == request.Email && c.TenantId == tenantId.Value,
                cancellationToken);

            if (existingCustomer != null)
            {
                return Result.Failure<CustomerResponse>(
                    Error.Conflict("Customer.DuplicateEmail", $"A customer with email '{request.Email}' already exists."));
            }
        }

        Address? address = null;
        if (!string.IsNullOrEmpty(request.Street) || !string.IsNullOrEmpty(request.City))
        {
            address = new Address(
                request.Street,
                null, // Street2
                request.City,
                request.State,
                request.PostalCode,
                request.Country);
        }

        var customer = new Customer
        {
            TenantId = tenantId.Value,
            Name = request.Name,
            CustomerGroupId = request.CustomerGroupId,
            Code = request.Code,
            Email = request.Email,
            Phone = request.Phone,
            AlternatePhone = request.AlternatePhone,
            Address = address,
            TaxIdentificationNo = request.TaxIdentificationNo,
            CreditLimit = request.CreditLimit,
            DateOfBirth = request.DateOfBirth,
            Notes = request.Notes,
            Tags = request.Tags,
            IsActive = true,
            CurrentBalance = 0,
            LoyaltyPoints = 0,
            TotalPurchases = 0,
            TotalOrders = 0
        };

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CustomerResponse(
            customer.Id,
            customer.Name,
            customer.Code,
            customer.Email,
            customer.Phone,
            customer.CurrentBalance,
            customer.LoyaltyPoints,
            customer.IsActive,
            customer.CreatedAt));
    }
}
