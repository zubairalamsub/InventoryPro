using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Customers.Queries;

public record GetCustomersQuery(
    string? SearchTerm = null,
    Guid? CustomerGroupId = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 10
) : IQuery<PagedList<CustomerListItemResponse>>;

public record CustomerListItemResponse(
    Guid Id,
    string Name,
    string? Code,
    string? Email,
    string? Phone,
    string? GroupName,
    decimal CurrentBalance,
    int LoyaltyPoints,
    decimal TotalPurchases,
    int TotalOrders,
    bool IsActive,
    DateTime CreatedAt);

public class GetCustomersQueryHandler : IQueryHandler<GetCustomersQuery, PagedList<CustomerListItemResponse>>
{
    private readonly IRepository<Customer> _customerRepository;

    public GetCustomersQueryHandler(IRepository<Customer> customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<Result<PagedList<CustomerListItemResponse>>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);

        var query = customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLowerInvariant();
            query = query.Where(c =>
                c.Name.ToLower().Contains(searchLower) ||
                (c.Code != null && c.Code.ToLower().Contains(searchLower)) ||
                (c.Email != null && c.Email.ToLower().Contains(searchLower)) ||
                (c.Phone != null && c.Phone.Contains(searchLower)));
        }

        if (request.CustomerGroupId.HasValue)
        {
            query = query.Where(c => c.CustomerGroupId == request.CustomerGroupId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        query = query.OrderBy(c => c.Name);

        var totalCount = query.Count();

        var pagedItems = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var items = pagedItems.Select(c => new CustomerListItemResponse(
            c.Id,
            c.Name,
            c.Code,
            c.Email,
            c.Phone,
            c.CustomerGroup?.Name,
            c.CurrentBalance,
            c.LoyaltyPoints,
            c.TotalPurchases,
            c.TotalOrders,
            c.IsActive,
            c.CreatedAt)).ToList();

        var pagedList = PagedList<CustomerListItemResponse>.Create(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result.Success(pagedList);
    }
}
