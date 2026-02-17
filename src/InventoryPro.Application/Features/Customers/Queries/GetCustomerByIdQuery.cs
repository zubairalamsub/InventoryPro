using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Customers.Queries;

public record GetCustomerByIdQuery(Guid Id) : IQuery<CustomerDetailResponse>;

public record CustomerDetailResponse(
    Guid Id,
    string Name,
    string? Code,
    Guid? CustomerGroupId,
    string? CustomerGroupName,
    string? Email,
    string? Phone,
    string? AlternatePhone,
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    string? TaxIdentificationNo,
    decimal? CreditLimit,
    decimal CurrentBalance,
    int LoyaltyPoints,
    decimal TotalPurchases,
    int TotalOrders,
    DateOnly? DateOfBirth,
    string? Notes,
    string[]? Tags,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID is required");
    }
}

public class GetCustomerByIdQueryHandler : IQueryHandler<GetCustomerByIdQuery, CustomerDetailResponse>
{
    private readonly IRepository<Customer> _customerRepository;

    public GetCustomerByIdQueryHandler(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<CustomerDetailResponse>> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
        {
            return Result.Failure<CustomerDetailResponse>(
                Error.NotFound("Customer", request.Id));
        }

        return Result.Success(new CustomerDetailResponse(
            customer.Id,
            customer.Name,
            customer.Code,
            customer.CustomerGroupId,
            customer.CustomerGroup?.Name,
            customer.Email,
            customer.Phone,
            customer.AlternatePhone,
            customer.Address?.Street,
            customer.Address?.City,
            customer.Address?.State,
            customer.Address?.PostalCode,
            customer.Address?.Country,
            customer.TaxIdentificationNo,
            customer.CreditLimit,
            customer.CurrentBalance,
            customer.LoyaltyPoints,
            customer.TotalPurchases,
            customer.TotalOrders,
            customer.DateOfBirth,
            customer.Notes,
            customer.Tags,
            customer.IsActive,
            customer.CreatedAt,
            customer.UpdatedAt));
    }
}
