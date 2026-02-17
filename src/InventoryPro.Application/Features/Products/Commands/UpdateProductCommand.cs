using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Products.Commands;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string SKU,
    decimal CostPrice,
    decimal SellingPrice,
    Guid? CategoryId = null,
    Guid? UnitOfMeasureId = null,
    string? Barcode = null,
    string? Description = null,
    string? ShortDescription = null,
    decimal? WholesalePrice = null,
    decimal? MinimumPrice = null,
    int ReorderLevel = 10,
    int ReorderQuantity = 50,
    int? MaxStockLevel = null,
    decimal? Weight = null,
    string? Dimensions = null,
    string[]? Tags = null,
    bool IsActive = true,
    bool IsService = false,
    bool TrackInventory = true,
    bool AllowNegativeStock = false
) : ICommand<ProductResponse>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.SKU)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters");

        RuleFor(x => x.CostPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Cost price must be non-negative");

        RuleFor(x => x.SellingPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Selling price must be non-negative");
    }
}

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, ProductResponse>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;

    public UpdateProductCommandHandler(
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
    }

    public async Task<Result<ProductResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null)
        {
            return Result.Failure<ProductResponse>(
                Error.NotFound("Product", request.Id));
        }

        // Check for duplicate SKU (excluding current product)
        var tenantId = _tenantProvider.GetTenantId();
        var existingProduct = await _productRepository.FirstOrDefaultAsync(
            p => p.SKU == request.SKU && p.TenantId == tenantId && p.Id != request.Id,
            cancellationToken);

        if (existingProduct != null)
        {
            return Result.Failure<ProductResponse>(
                Error.Conflict("Product.DuplicateSKU", $"A product with SKU '{request.SKU}' already exists."));
        }

        product.Name = request.Name;
        product.SKU = request.SKU;
        product.Barcode = request.Barcode;
        product.Description = request.Description;
        product.ShortDescription = request.ShortDescription;
        product.CategoryId = request.CategoryId;
        product.UnitOfMeasureId = request.UnitOfMeasureId;
        product.CostPrice = request.CostPrice;
        product.SellingPrice = request.SellingPrice;
        product.WholesalePrice = request.WholesalePrice;
        product.MinimumPrice = request.MinimumPrice;
        product.ReorderLevel = request.ReorderLevel;
        product.ReorderQuantity = request.ReorderQuantity;
        product.MaxStockLevel = request.MaxStockLevel;
        product.Weight = request.Weight;
        product.Dimensions = request.Dimensions;
        product.Tags = request.Tags;
        product.IsActive = request.IsActive;
        product.IsService = request.IsService;
        product.TrackInventory = request.TrackInventory;
        product.AllowNegativeStock = request.AllowNegativeStock;

        _productRepository.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ProductResponse(
            product.Id,
            product.Name,
            product.SKU,
            product.Barcode,
            product.CostPrice,
            product.SellingPrice,
            product.ReorderLevel,
            product.IsActive,
            product.CreatedAt));
    }
}
