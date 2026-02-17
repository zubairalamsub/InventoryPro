using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Inventory.Queries;

public record GetWarehousesQuery(
    string? SearchTerm = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 10
) : IQuery<PagedList<WarehouseListItemResponse>>;

public record WarehouseListItemResponse(
    Guid Id,
    string Name,
    string? Code,
    string? City,
    string? Phone,
    bool IsDefault,
    bool IsActive,
    int TotalProducts,
    DateTime CreatedAt);

public class GetWarehousesQueryHandler : IQueryHandler<GetWarehousesQuery, PagedList<WarehouseListItemResponse>>
{
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IRepository<StockLevel> _stockLevelRepository;

    public GetWarehousesQueryHandler(
        IRepository<Warehouse> warehouseRepository,
        IRepository<StockLevel> stockLevelRepository)
    {
        _warehouseRepository = warehouseRepository;
        _stockLevelRepository = stockLevelRepository;
    }

    public async Task<Result<PagedList<WarehouseListItemResponse>>> Handle(GetWarehousesQuery request, CancellationToken cancellationToken)
    {
        var warehouses = await _warehouseRepository.GetAllAsync(cancellationToken);
        var stockLevels = await _stockLevelRepository.GetAllAsync(cancellationToken);

        var query = warehouses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLowerInvariant();
            query = query.Where(w =>
                w.Name.ToLower().Contains(searchLower) ||
                (w.Code != null && w.Code.ToLower().Contains(searchLower)));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(w => w.IsActive == request.IsActive.Value);
        }

        query = query.OrderBy(w => w.Name);

        var totalCount = query.Count();

        var stockByWarehouse = stockLevels
            .Where(sl => sl.Quantity > 0)
            .GroupBy(sl => sl.WarehouseId)
            .ToDictionary(g => g.Key, g => g.Select(sl => sl.ProductId).Distinct().Count());

        var pagedItems = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var items = pagedItems.Select(w => new WarehouseListItemResponse(
            w.Id,
            w.Name,
            w.Code,
            w.Address?.City,
            w.Phone,
            w.IsDefault,
            w.IsActive,
            stockByWarehouse.GetValueOrDefault(w.Id, 0),
            w.CreatedAt)).ToList();

        var pagedList = PagedList<WarehouseListItemResponse>.Create(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result.Success(pagedList);
    }
}
