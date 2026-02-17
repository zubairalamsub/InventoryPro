using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Common.ValueObjects;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Customers.Commands;

public record UpdateCustomerCommand(
    Guid Id,
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
    string[]? Tags = null,
    bool IsActive = true
) : ICommand<CustomerResponse>;

public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required")
            .MaximumLength(200).WithMessage("Customer name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email address")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class UpdateCustomerCommandHandler : ICommandHandler<UpdateCustomerCommand, CustomerResponse>
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;

    public UpdateCustomerCommandHandler(
        IRepository<Customer> customerRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
    }

    public async Task<Result<CustomerResponse>> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
        {
            return Result.Failure<CustomerResponse>(
                Error.NotFound("Customer", request.Id));
        }

        var tenantId = _tenantProvider.GetTenantId();

        // Check for duplicate code within tenant (excluding current)
        if (!string.IsNullOrEmpty(request.Code))
        {
            var existingCustomer = await _customerRepository.FirstOrDefaultAsync(
                c => c.Code == request.Code && c.TenantId == tenantId && c.Id != request.Id,
                cancellationToken);

            if (existingCustomer != null)
            {
                return Result.Failure<CustomerResponse>(
                    Error.Conflict("Customer.DuplicateCode", $"A customer with code '{request.Code}' already exists."));
            }
        }

        // Check for duplicate email within tenant (excluding current)
        if (!string.IsNullOrEmpty(request.Email))
        {
            var existingCustomer = await _customerRepository.FirstOrDefaultAsync(
                c => c.Email == request.Email && c.TenantId == tenantId && c.Id != request.Id,
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

        customer.Name = request.Name;
        customer.CustomerGroupId = request.CustomerGroupId;
        customer.Code = request.Code;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.AlternatePhone = request.AlternatePhone;
        customer.Address = address;
        customer.TaxIdentificationNo = request.TaxIdentificationNo;
        customer.CreditLimit = request.CreditLimit;
        customer.DateOfBirth = request.DateOfBirth;
        customer.Notes = request.Notes;
        customer.Tags = request.Tags;
        customer.IsActive = request.IsActive;

        _customerRepository.Update(customer);
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
