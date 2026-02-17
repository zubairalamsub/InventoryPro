using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Products.Queries;

public record GetProductByIdQuery(Guid Id) : IQuery<ProductDetailResponse>;

public record ProductDetailResponse(
    Guid Id,
    string Name,
    string SKU,
    string? Barcode,
    string? Description,
    string? ShortDescription,
    decimal CostPrice,
    decimal SellingPrice,
    decimal? WholesalePrice,
    decimal? MinimumPrice,
    int ReorderLevel,
    int ReorderQuantity,
    int? MaxStockLevel,
    decimal? Weight,
    string? Dimensions,
    string[]? Tags,
    bool IsActive,
    bool IsService,
    bool TrackInventory,
    bool AllowNegativeStock,
    Guid? CategoryId,
    string? CategoryName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");
    }
}

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductDetailResponse>
{
    private readonly IRepository<Product> _productRepository;

    public GetProductByIdQueryHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDetailResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result.Failure<ProductDetailResponse>(
                Error.NotFound("Product", request.Id));
        }

        return Result.Success(new ProductDetailResponse(
            product.Id,
            product.Name,
            product.SKU,
            product.Barcode,
            product.Description,
            product.ShortDescription,
            product.CostPrice,
            product.SellingPrice,
            product.WholesalePrice,
            product.MinimumPrice,
            product.ReorderLevel,
            product.ReorderQuantity,
            product.MaxStockLevel,
            product.Weight,
            product.Dimensions,
            product.Tags,
            product.IsActive,
            product.IsService,
            product.TrackInventory,
            product.AllowNegativeStock,
            product.CategoryId,
            product.Category?.Name,
            product.CreatedAt,
            product.UpdatedAt));
    }
}
