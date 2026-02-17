using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Inventory.Commands;

public record CreateStockTransferCommand(
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    List<StockTransferItemDto> Items,
    string? Notes = null
) : ICommand<StockTransferResponse>;

public record StockTransferItemDto(
    Guid ProductId,
    int Quantity,
    Guid? ProductVariantId = null,
    string? Notes = null);

public record StockTransferResponse(
    Guid Id,
    string TransferNumber,
    Guid FromWarehouseId,
    string FromWarehouseName,
    Guid ToWarehouseId,
    string ToWarehouseName,
    StockTransferStatus Status,
    int ItemCount,
    DateTime TransferDate);

public class CreateStockTransferCommandValidator : AbstractValidator<CreateStockTransferCommand>
{
    public CreateStockTransferCommandValidator()
    {
        RuleFor(x => x.FromWarehouseId)
            .NotEmpty().WithMessage("Source warehouse ID is required");

        RuleFor(x => x.ToWarehouseId)
            .NotEmpty().WithMessage("Destination warehouse ID is required")
            .NotEqual(x => x.FromWarehouseId).WithMessage("Source and destination warehouses must be different");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero");
        });
    }
}

public class CreateStockTransferCommandHandler : ICommandHandler<CreateStockTransferCommand, StockTransferResponse>
{
    private readonly IRepository<StockTransfer> _stockTransferRepository;
    private readonly IRepository<StockLevel> _stockLevelRepository;
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<InventoryTransaction> _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public CreateStockTransferCommandHandler(
        IRepository<StockTransfer> stockTransferRepository,
        IRepository<StockLevel> stockLevelRepository,
        IRepository<Warehouse> warehouseRepository,
        IRepository<Product> productRepository,
        IRepository<InventoryTransaction> transactionRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        IDateTime dateTime)
    {
        _stockTransferRepository = stockTransferRepository;
        _stockLevelRepository = stockLevelRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public async Task<Result<StockTransferResponse>> Handle(CreateStockTransferCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (!tenantId.HasValue)
        {
            return Result.Failure<StockTransferResponse>(
                Error.Unauthorized("Tenant context is required."));
        }

        var fromWarehouse = await _warehouseRepository.GetByIdAsync(request.FromWarehouseId, cancellationToken);
        if (fromWarehouse == null)
        {
            return Result.Failure<StockTransferResponse>(
                Error.NotFound("Warehouse", request.FromWarehouseId));
        }

        var toWarehouse = await _warehouseRepository.GetByIdAsync(request.ToWarehouseId, cancellationToken);
        if (toWarehouse == null)
        {
            return Result.Failure<StockTransferResponse>(
                Error.NotFound("Warehouse", request.ToWarehouseId));
        }

        var transferNumber = $"TRF-{_dateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var transfer = new StockTransfer
        {
            TenantId = tenantId.Value,
            TransferNumber = transferNumber,
            FromWarehouseId = request.FromWarehouseId,
            ToWarehouseId = request.ToWarehouseId,
            Status = StockTransferStatus.Pending,
            Notes = request.Notes,
            TransferredBy = _currentUserService.UserId ?? Guid.Empty,
            TransferDate = _dateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null)
            {
                return Result.Failure<StockTransferResponse>(
                    Error.NotFound("Product", item.ProductId));
            }

            // Check stock in source warehouse
            var sourceStockLevel = await _stockLevelRepository.FirstOrDefaultAsync(
                sl => sl.ProductId == item.ProductId &&
                      sl.WarehouseId == request.FromWarehouseId &&
                      sl.ProductVariantId == item.ProductVariantId,
                cancellationToken);

            var availableQuantity = sourceStockLevel?.AvailableQuantity ?? 0;

            if (availableQuantity < item.Quantity)
            {
                return Result.Failure<StockTransferResponse>(
                    Error.Validation("StockTransfer.InsufficientStock",
                        $"Insufficient stock for product '{product.Name}'. Available: {availableQuantity}, Requested: {item.Quantity}"));
            }

            // Reserve the stock in source warehouse
            sourceStockLevel!.ReservedQuantity += item.Quantity;
            _stockLevelRepository.Update(sourceStockLevel);

            // Add transfer item
            transfer.Items.Add(new StockTransferItem
            {
                ProductId = item.ProductId,
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                Notes = item.Notes
            });
        }

        await _stockTransferRepository.AddAsync(transfer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new StockTransferResponse(
            transfer.Id,
            transfer.TransferNumber,
            transfer.FromWarehouseId,
            fromWarehouse.Name,
            transfer.ToWarehouseId,
            toWarehouse.Name,
            transfer.Status,
            transfer.Items.Count,
            transfer.TransferDate));
    }
}
