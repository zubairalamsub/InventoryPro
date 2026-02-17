using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Inventory.Queries;

public record GetStockLevelsQuery(
    Guid? WarehouseId = null,
    Guid? ProductId = null,
    bool? LowStockOnly = null,
    int PageNumber = 1,
    int PageSize = 10
) : IQuery<PagedList<StockLevelResponse>>;

public record StockLevelResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    Guid WarehouseId,
    string WarehouseName,
    int Quantity,
    int ReservedQuantity,
    int AvailableQuantity,
    int ReorderLevel,
    bool IsLowStock,
    DateTime LastUpdated);

public class GetStockLevelsQueryHandler : IQueryHandler<GetStockLevelsQuery, PagedList<StockLevelResponse>>
{
    private readonly IRepository<StockLevel> _stockLevelRepository;
    private readonly IRepository<Product> _productRepository;

    public GetStockLevelsQueryHandler(
        IRepository<StockLevel> stockLevelRepository,
        IRepository<Product> productRepository)
    {
        _stockLevelRepository = stockLevelRepository;
        _productRepository = productRepository;
    }

    public async Task<Result<PagedList<StockLevelResponse>>> Handle(GetStockLevelsQuery request, CancellationToken cancellationToken)
    {
        var stockLevels = await _stockLevelRepository.GetAllAsync(cancellationToken);
        var products = await _productRepository.GetAllAsync(cancellationToken);

        var productDict = products.ToDictionary(p => p.Id);

        var query = stockLevels.AsQueryable();

        if (request.WarehouseId.HasValue)
        {
            query = query.Where(sl => sl.WarehouseId == request.WarehouseId.Value);
        }

        if (request.ProductId.HasValue)
        {
            query = query.Where(sl => sl.ProductId == request.ProductId.Value);
        }

        var queryList = query.ToList();

        // Apply low stock filter
        if (request.LowStockOnly == true)
        {
            queryList = queryList.Where(sl =>
            {
                if (productDict.TryGetValue(sl.ProductId, out var product))
                {
                    return sl.Quantity <= product.ReorderLevel;
                }
                return false;
            }).ToList();
        }

        var totalCount = queryList.Count;

        var items = queryList
            .OrderBy(sl => sl.Product?.Name ?? string.Empty)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(sl =>
            {
                var product = productDict.GetValueOrDefault(sl.ProductId);
                var reorderLevel = product?.ReorderLevel ?? 0;
                return new StockLevelResponse(
                    sl.Id,
                    sl.ProductId,
                    product?.Name ?? "Unknown",
                    product?.SKU ?? "Unknown",
                    sl.WarehouseId,
                    sl.Warehouse?.Name ?? "Unknown",
                    sl.Quantity,
                    sl.ReservedQuantity,
                    sl.AvailableQuantity,
                    reorderLevel,
                    sl.Quantity <= reorderLevel,
                    sl.LastUpdated);
            })
            .ToList();

        var pagedList = PagedList<StockLevelResponse>.Create(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result.Success(pagedList);
    }
}
