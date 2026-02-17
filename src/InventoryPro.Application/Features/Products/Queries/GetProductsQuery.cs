using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Products.Queries;

public record GetProductsQuery(
    string? SearchTerm = null,
    Guid? CategoryId = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 10
) : IQuery<PagedList<ProductListItemResponse>>;

public record ProductListItemResponse(
    Guid Id,
    string Name,
    string SKU,
    string? Barcode,
    decimal CostPrice,
    decimal SellingPrice,
    int ReorderLevel,
    bool IsActive,
    string? CategoryName,
    DateTime CreatedAt);

public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, PagedList<ProductListItemResponse>>
{
    private readonly IRepository<Product> _productRepository;

    public GetProductsQueryHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<PagedList<ProductListItemResponse>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);

        // Apply filters
        var query = products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchLower = request.SearchTerm.ToLowerInvariant();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchLower) ||
                p.SKU.ToLower().Contains(searchLower) ||
                (p.Barcode != null && p.Barcode.ToLower().Contains(searchLower)));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        // Order by name
        query = query.OrderBy(p => p.Name);

        var totalCount = query.Count();
        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductListItemResponse(
                p.Id,
                p.Name,
                p.SKU,
                p.Barcode,
                p.CostPrice,
                p.SellingPrice,
                p.ReorderLevel,
                p.IsActive,
                p.Category != null ? p.Category.Name : null,
                p.CreatedAt))
            .ToList();

        var pagedList = PagedList<ProductListItemResponse>.Create(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount);

        return Result.Success(pagedList);
    }
}
