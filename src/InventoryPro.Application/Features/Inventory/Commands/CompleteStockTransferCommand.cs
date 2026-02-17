using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Inventory.Commands;

public record CompleteStockTransferCommand(Guid Id) : ICommand<StockTransferResponse>;

public class CompleteStockTransferCommandValidator : AbstractValidator<CompleteStockTransferCommand>
{
    public CompleteStockTransferCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Transfer ID is required");
    }
}

public class CompleteStockTransferCommandHandler : ICommandHandler<CompleteStockTransferCommand, StockTransferResponse>
{
    private readonly IRepository<StockTransfer> _stockTransferRepository;
    private readonly IRepository<StockLevel> _stockLevelRepository;
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IRepository<InventoryTransaction> _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public CompleteStockTransferCommandHandler(
        IRepository<StockTransfer> stockTransferRepository,
        IRepository<StockLevel> stockLevelRepository,
        IRepository<Warehouse> warehouseRepository,
        IRepository<InventoryTransaction> transactionRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        IDateTime dateTime)
    {
        _stockTransferRepository = stockTransferRepository;
        _stockLevelRepository = stockLevelRepository;
        _warehouseRepository = warehouseRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public async Task<Result<StockTransferResponse>> Handle(CompleteStockTransferCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (!tenantId.HasValue)
        {
            return Result.Failure<StockTransferResponse>(
                Error.Unauthorized("Tenant context is required."));
        }

        var transfer = await _stockTransferRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transfer == null)
        {
            return Result.Failure<StockTransferResponse>(
                Error.NotFound("StockTransfer", request.Id));
        }

        if (transfer.Status != StockTransferStatus.Pending && transfer.Status != StockTransferStatus.InTransit)
        {
            return Result.Failure<StockTransferResponse>(
                Error.Validation("StockTransfer.InvalidStatus", $"Cannot complete transfer with status '{transfer.Status}'."));
        }

        var fromWarehouse = await _warehouseRepository.GetByIdAsync(transfer.FromWarehouseId, cancellationToken);
        var toWarehouse = await _warehouseRepository.GetByIdAsync(transfer.ToWarehouseId, cancellationToken);

        // Load items if not loaded
        var items = await _stockTransferRepository.GetAllAsync(cancellationToken);
        var transferWithItems = items.FirstOrDefault(t => t.Id == request.Id);

        if (transferWithItems?.Items == null || !transferWithItems.Items.Any())
        {
            return Result.Failure<StockTransferResponse>(
                Error.Validation("StockTransfer.NoItems", "Transfer has no items."));
        }

        foreach (var item in transferWithItems.Items)
        {
            // Deduct from source warehouse
            var sourceStockLevel = await _stockLevelRepository.FirstOrDefaultAsync(
                sl => sl.ProductId == item.ProductId &&
                      sl.WarehouseId == transfer.FromWarehouseId &&
                      sl.ProductVariantId == item.ProductVariantId,
                cancellationToken);

            if (sourceStockLevel != null)
            {
                sourceStockLevel.Quantity -= item.Quantity;
                sourceStockLevel.ReservedQuantity -= item.Quantity;
                sourceStockLevel.LastUpdated = _dateTime.UtcNow;
                _stockLevelRepository.Update(sourceStockLevel);

                // Create outgoing transaction
                await _transactionRepository.AddAsync(new InventoryTransaction
                {
                    TenantId = tenantId.Value,
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    WarehouseId = transfer.FromWarehouseId,
                    Type = InventoryTransactionType.Transfer,
                    Quantity = -item.Quantity,
                    RunningBalance = sourceStockLevel.Quantity,
                    ReferenceType = nameof(StockTransfer),
                    ReferenceId = transfer.Id,
                    Notes = $"Transfer out to {toWarehouse?.Name}"
                }, cancellationToken);
            }

            // Add to destination warehouse
            var destStockLevel = await _stockLevelRepository.FirstOrDefaultAsync(
                sl => sl.ProductId == item.ProductId &&
                      sl.WarehouseId == transfer.ToWarehouseId &&
                      sl.ProductVariantId == item.ProductVariantId,
                cancellationToken);

            if (destStockLevel == null)
            {
                destStockLevel = new StockLevel
                {
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    WarehouseId = transfer.ToWarehouseId,
                    Quantity = item.Quantity,
                    ReservedQuantity = 0,
                    LastUpdated = _dateTime.UtcNow
                };
                await _stockLevelRepository.AddAsync(destStockLevel, cancellationToken);
            }
            else
            {
                destStockLevel.Quantity += item.Quantity;
                destStockLevel.LastUpdated = _dateTime.UtcNow;
                _stockLevelRepository.Update(destStockLevel);
            }

            // Create incoming transaction
            await _transactionRepository.AddAsync(new InventoryTransaction
            {
                TenantId = tenantId.Value,
                ProductId = item.ProductId,
                ProductVariantId = item.ProductVariantId,
                WarehouseId = transfer.ToWarehouseId,
                Type = InventoryTransactionType.Transfer,
                Quantity = item.Quantity,
                RunningBalance = destStockLevel.Quantity,
                ReferenceType = nameof(StockTransfer),
                ReferenceId = transfer.Id,
                Notes = $"Transfer in from {fromWarehouse?.Name}"
            }, cancellationToken);
        }

        transfer.Status = StockTransferStatus.Completed;
        transfer.ReceivedBy = _currentUserService.UserId;
        transfer.ReceivedDate = _dateTime.UtcNow;
        _stockTransferRepository.Update(transfer);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new StockTransferResponse(
            transfer.Id,
            transfer.TransferNumber,
            transfer.FromWarehouseId,
            fromWarehouse?.Name ?? string.Empty,
            transfer.ToWarehouseId,
            toWarehouse?.Name ?? string.Empty,
            transfer.Status,
            transferWithItems.Items.Count,
            transfer.TransferDate));
    }
}
