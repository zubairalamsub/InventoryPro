using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Sales.Queries;

public record GetSalesQuery(
    string? SearchTerm = null,
    Guid? CustomerId = null,
    Guid? WarehouseId = null,
    SaleStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int PageNumber = 1,
    int PageSize = 10
) : IQuery<PagedList<SaleListItemResponse>>;

public record SaleListItemResponse(
    Guid Id,
    string InvoiceNumber,
    Guid WarehouseId,
    string WarehouseName,
    Guid? CustomerId,
    string? CustomerName,
    SaleStatus Status,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    int ItemCount,
    DateTime SaleDate);

public class GetSalesQueryHandler : IQueryHandler<GetSalesQuery, PagedList<SaleListItemResponse>>
{
    private readonly IRepository<Sale> _saleRepository;

    public GetSalesQueryHandler(IRepository<Sale> saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<Result<PagedList<SaleListItemResponse>>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var sales = await _saleRepository.GetAllAsync(cancellationToken);

        var query = sales.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLowerInvariant();
            query = query.Where(s =>
                s.InvoiceNumber.ToLower().Contains(searchLower) ||
                (s.Customer != null && s.Customer.Name.ToLower().Contains(searchLower)));
        }

        if (request.CustomerId.HasValue)
        {
            query = query.Where(s => s.CustomerId == request.CustomerId.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(s => s.WarehouseId == request.WarehouseId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(s => s.Status == request.Status.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(s => s.SaleDate >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(s => s.SaleDate <= request.ToDate.Value);
        }

        query = query.OrderByDescending(s => s.SaleDate);

        var totalCount = query.Count();

        var pagedItems = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var items = pagedItems.Select(s => new SaleListItemResponse(
            s.Id,
            s.InvoiceNumber,
            s.WarehouseId,
            s.Warehouse?.Name ?? "Unknown",
            s.CustomerId,
            s.Customer?.Name,
            s.Status,
            s.TotalAmount,
            s.PaidAmount,
            s.OutstandingAmount,
            s.Items?.Count ?? 0,
            s.SaleDate)).ToList();

        var pagedList = PagedList<SaleListItemResponse>.Create(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result.Success(pagedList);
    }
}
