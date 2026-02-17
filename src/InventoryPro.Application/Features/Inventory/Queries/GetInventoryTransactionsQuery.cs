using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Inventory.Queries;

public record GetInventoryTransactionsQuery(
    Guid? ProductId = null,
    Guid? WarehouseId = null,
    InventoryTransactionType? Type = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int PageNumber = 1,
    int PageSize = 20
) : IQuery<PagedList<InventoryTransactionResponse>>;

public record InventoryTransactionResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    Guid WarehouseId,
    string WarehouseName,
    InventoryTransactionType Type,
    int Quantity,
    int RunningBalance,
    string? ReferenceType,
    Guid? ReferenceId,
    string? Notes,
    DateTime CreatedAt);

public class GetInventoryTransactionsQueryHandler : IQueryHandler<GetInventoryTransactionsQuery, PagedList<InventoryTransactionResponse>>
{
    private readonly IRepository<InventoryTransaction> _transactionRepository;

    public GetInventoryTransactionsQueryHandler(IRepository<InventoryTransaction> transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<PagedList<InventoryTransactionResponse>>> Handle(GetInventoryTransactionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _transactionRepository.GetAllAsync(cancellationToken);

        var query = transactions.AsQueryable();

        if (request.ProductId.HasValue)
        {
            query = query.Where(t => t.ProductId == request.ProductId.Value);
        }

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(t => t.WarehouseId == request.WarehouseId.Value);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(t => t.Type == request.Type.Value);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= request.ToDate.Value);
        }

        query = query.OrderByDescending(t => t.CreatedAt);

        var totalCount = query.Count();

        var pagedItems = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var items = pagedItems.Select(t => new InventoryTransactionResponse(
            t.Id,
            t.ProductId,
            t.Product?.Name ?? "Unknown",
            t.Product?.SKU ?? "Unknown",
            t.WarehouseId,
            t.Warehouse?.Name ?? "Unknown",
            t.Type,
            t.Quantity,
            t.RunningBalance,
            t.ReferenceType,
            t.ReferenceId,
            t.Notes,
            t.CreatedAt)).ToList();

        var pagedList = PagedList<InventoryTransactionResponse>.Create(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result.Success(pagedList);
    }
}
