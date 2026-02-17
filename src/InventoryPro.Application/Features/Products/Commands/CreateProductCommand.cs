using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Products.Commands;

public record CreateProductCommand(
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
    bool IsService = false,
    bool TrackInventory = true,
    bool AllowNegativeStock = false
) : ICommand<ProductResponse>;

public record ProductResponse(
    Guid Id,
    string Name,
    string SKU,
    string? Barcode,
    decimal CostPrice,
    decimal SellingPrice,
    int ReorderLevel,
    bool IsActive,
    DateTime CreatedAt);

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
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

        RuleFor(x => x.ReorderLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder level must be non-negative");

        RuleFor(x => x.ReorderQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Reorder quantity must be non-negative");
    }
}

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, ProductResponse>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly IDateTime _dateTime;

    public CreateProductCommandHandler(
        IRepository<Product> productRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        IDateTime dateTime)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _dateTime = dateTime;
    }

    public async Task<Result<ProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (!tenantId.HasValue)
        {
            return Result.Failure<ProductResponse>(
                Error.Unauthorized("Tenant context is required."));
        }

        // Check for duplicate SKU within tenant
        var existingProduct = await _productRepository.FirstOrDefaultAsync(
            p => p.SKU == request.SKU && p.TenantId == tenantId.Value,
            cancellationToken);

        if (existingProduct != null)
        {
            return Result.Failure<ProductResponse>(
                Error.Conflict("Product.DuplicateSKU", $"A product with SKU '{request.SKU}' already exists."));
        }

        var product = new Product
        {
            TenantId = tenantId.Value,
            Name = request.Name,
            SKU = request.SKU,
            Barcode = request.Barcode,
            Description = request.Description,
            ShortDescription = request.ShortDescription,
            CategoryId = request.CategoryId,
            UnitOfMeasureId = request.UnitOfMeasureId,
            CostPrice = request.CostPrice,
            SellingPrice = request.SellingPrice,
            WholesalePrice = request.WholesalePrice,
            MinimumPrice = request.MinimumPrice,
            ReorderLevel = request.ReorderLevel,
            ReorderQuantity = request.ReorderQuantity,
            MaxStockLevel = request.MaxStockLevel,
            Weight = request.Weight,
            Dimensions = request.Dimensions,
            Tags = request.Tags,
            IsService = request.IsService,
            TrackInventory = request.TrackInventory,
            AllowNegativeStock = request.AllowNegativeStock,
            IsActive = true,
            Slug = GenerateSlug(request.Name)
        };

        await _productRepository.AddAsync(product, cancellationToken);
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

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace("\"", "");
    }
}
