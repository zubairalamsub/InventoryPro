using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Inventory.Commands;

public record CreateStockAdjustmentCommand(
    Guid WarehouseId,
    AdjustmentReason Reason,
    List<StockAdjustmentItemDto> Items,
    string? Notes = null
) : ICommand<StockAdjustmentResponse>;

public record StockAdjustmentItemDto(
    Guid ProductId,
    int QuantityAdjusted,
    Guid? ProductVariantId = null,
    string? Notes = null);

public record StockAdjustmentResponse(
    Guid Id,
    string AdjustmentNumber,
    Guid WarehouseId,
    string WarehouseName,
    AdjustmentReason Reason,
    int ItemCount,
    DateTime AdjustmentDate);

public class CreateStockAdjustmentCommandValidator : AbstractValidator<CreateStockAdjustmentCommand>
{
    public CreateStockAdjustmentCommandValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse ID is required");

        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage("Invalid adjustment reason");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            item.RuleFor(x => x.QuantityAdjusted)
                .NotEqual(0).WithMessage("Quantity adjustment cannot be zero");
        });
    }
}

public class CreateStockAdjustmentCommandHandler : ICommandHandler<CreateStockAdjustmentCommand, StockAdjustmentResponse>
{
    private readonly IRepository<StockAdjustment> _stockAdjustmentRepository;
    private readonly IRepository<StockLevel> _stockLevelRepository;
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<InventoryTransaction> _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public CreateStockAdjustmentCommandHandler(
        IRepository<StockAdjustment> stockAdjustmentRepository,
        IRepository<StockLevel> stockLevelRepository,
        IRepository<Warehouse> warehouseRepository,
        IRepository<Product> productRepository,
        IRepository<InventoryTransaction> transactionRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        IDateTime dateTime)
    {
        _stockAdjustmentRepository = stockAdjustmentRepository;
        _stockLevelRepository = stockLevelRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public async Task<Result<StockAdjustmentResponse>> Handle(CreateStockAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (!tenantId.HasValue)
        {
            return Result.Failure<StockAdjustmentResponse>(
                Error.Unauthorized("Tenant context is required."));
        }

        var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId, cancellationToken);
        if (warehouse == null)
        {
            return Result.Failure<StockAdjustmentResponse>(
                Error.NotFound("Warehouse", request.WarehouseId));
        }

        var adjustmentNumber = $"ADJ-{_dateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var adjustment = new StockAdjustment
        {
            TenantId = tenantId.Value,
            AdjustmentNumber = adjustmentNumber,
            WarehouseId = request.WarehouseId,
            Reason = request.Reason,
            Notes = request.Notes,
            AdjustedBy = _currentUserService.UserId ?? Guid.Empty,
            AdjustmentDate = _dateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null)
            {
                return Result.Failure<StockAdjustmentResponse>(
                    Error.NotFound("Product", item.ProductId));
            }

            // Get or create stock level
            var stockLevel = await _stockLevelRepository.FirstOrDefaultAsync(
                sl => sl.ProductId == item.ProductId &&
                      sl.WarehouseId == request.WarehouseId &&
                      sl.ProductVariantId == item.ProductVariantId,
                cancellationToken);

            var quantityBefore = stockLevel?.Quantity ?? 0;
            var quantityAfter = quantityBefore + item.QuantityAdjusted;

            if (quantityAfter < 0 && !product.AllowNegativeStock)
            {
                return Result.Failure<StockAdjustmentResponse>(
                    Error.Validation("StockAdjustment.InsufficientStock",
                        $"Insufficient stock for product '{product.Name}'. Available: {quantityBefore}, Adjustment: {item.QuantityAdjusted}"));
            }

            // Create or update stock level
            if (stockLevel == null)
            {
                stockLevel = new StockLevel
                {
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    WarehouseId = request.WarehouseId,
                    Quantity = item.QuantityAdjusted,
                    ReservedQuantity = 0,
                    LastUpdated = _dateTime.UtcNow
                };
                await _stockLevelRepository.AddAsync(stockLevel, cancellationToken);
            }
            else
            {
                stockLevel.Quantity = quantityAfter;
                stockLevel.LastUpdated = _dateTime.UtcNow;
                _stockLevelRepository.Update(stockLevel);
            }

            // Add adjustment item
            adjustment.Items.Add(new StockAdjustmentItem
            {
                ProductId = item.ProductId,
                ProductVariantId = item.ProductVariantId,
                QuantityBefore = quantityBefore,
                QuantityAfter = quantityAfter,
                QuantityAdjusted = item.QuantityAdjusted,
                Notes = item.Notes
            });

            // Create inventory transaction
            var transaction = new InventoryTransaction
            {
                TenantId = tenantId.Value,
                ProductId = item.ProductId,
                ProductVariantId = item.ProductVariantId,
                WarehouseId = request.WarehouseId,
                Type = InventoryTransactionType.Adjustment,
                Quantity = item.QuantityAdjusted,
                RunningBalance = quantityAfter,
                ReferenceType = nameof(StockAdjustment),
                ReferenceId = adjustment.Id,
                Reason = request.Reason,
                Notes = item.Notes
            };
            await _transactionRepository.AddAsync(transaction, cancellationToken);
        }

        await _stockAdjustmentRepository.AddAsync(adjustment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new StockAdjustmentResponse(
            adjustment.Id,
            adjustment.AdjustmentNumber,
            adjustment.WarehouseId,
            warehouse.Name,
            adjustment.Reason,
            adjustment.Items.Count,
            adjustment.AdjustmentDate));
    }
}
