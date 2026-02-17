using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Sales.Commands;

public record VoidSaleCommand(Guid Id, string? Reason = null) : ICommand;

public class VoidSaleCommandValidator : AbstractValidator<VoidSaleCommand>
{
    public VoidSaleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Sale ID is required");
    }
}

public class VoidSaleCommandHandler : ICommandHandler<VoidSaleCommand>
{
    private readonly IRepository<Sale> _saleRepository;
    private readonly IRepository<StockLevel> _stockLevelRepository;
    private readonly IRepository<InventoryTransaction> _transactionRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly IDateTime _dateTime;

    public VoidSaleCommandHandler(
        IRepository<Sale> saleRepository,
        IRepository<StockLevel> stockLevelRepository,
        IRepository<InventoryTransaction> transactionRepository,
        IRepository<Customer> customerRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        IDateTime dateTime)
    {
        _saleRepository = saleRepository;
        _stockLevelRepository = stockLevelRepository;
        _transactionRepository = transactionRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _dateTime = dateTime;
    }

    public async Task<Result> Handle(VoidSaleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (!tenantId.HasValue)
        {
            return Result.Failure(
                Error.Unauthorized("Tenant context is required."));
        }

        var sales = await _saleRepository.GetAllAsync(cancellationToken);
        var sale = sales.FirstOrDefault(s => s.Id == request.Id);

        if (sale == null)
        {
            return Result.Failure(
                Error.NotFound("Sale", request.Id));
        }

        if (sale.Status == SaleStatus.Voided)
        {
            return Result.Failure(
                Error.Validation("Sale.AlreadyVoided", "This sale has already been voided."));
        }

        if (sale.Status == SaleStatus.Returned)
        {
            return Result.Failure(
                Error.Validation("Sale.AlreadyReturned", "Cannot void a fully returned sale."));
        }

        // Restore stock for each item
        foreach (var item in sale.Items)
        {
            var stockLevel = await _stockLevelRepository.FirstOrDefaultAsync(
                sl => sl.ProductId == item.ProductId &&
                      sl.WarehouseId == sale.WarehouseId &&
                      sl.ProductVariantId == item.ProductVariantId,
                cancellationToken);

            if (stockLevel != null)
            {
                stockLevel.Quantity += item.Quantity;
                stockLevel.LastUpdated = _dateTime.UtcNow;
                _stockLevelRepository.Update(stockLevel);

                // Create reversal transaction
                await _transactionRepository.AddAsync(new InventoryTransaction
                {
                    TenantId = tenantId.Value,
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    WarehouseId = sale.WarehouseId,
                    Type = InventoryTransactionType.Return,
                    Quantity = item.Quantity,
                    UnitCost = item.CostPrice,
                    TotalCost = item.CostPrice * item.Quantity,
                    RunningBalance = stockLevel.Quantity,
                    ReferenceType = nameof(Sale),
                    ReferenceId = sale.Id,
                    Notes = $"Void sale: {sale.InvoiceNumber}. {request.Reason}"
                }, cancellationToken);
            }
        }

        // Update customer stats
        if (sale.CustomerId.HasValue)
        {
            var customer = await _customerRepository.GetByIdAsync(sale.CustomerId.Value, cancellationToken);
            if (customer != null)
            {
                customer.TotalOrders--;
                customer.TotalPurchases -= sale.TotalAmount;
                _customerRepository.Update(customer);
            }
        }

        sale.Status = SaleStatus.Voided;
        sale.InternalNotes = $"{sale.InternalNotes}\n[VOIDED] {_dateTime.UtcNow:yyyy-MM-dd HH:mm}: {request.Reason}".Trim();
        _saleRepository.Update(sale);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
