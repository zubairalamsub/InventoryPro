using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Sales.Commands;

public record CreateSaleCommand(
    Guid WarehouseId,
    List<SaleItemDto> Items,
    Guid? CustomerId = null,
    decimal DiscountAmount = 0,
    decimal ShippingAmount = 0,
    decimal PaidAmount = 0,
    string? Notes = null,
    string? CouponCode = null
) : ICommand<SaleResponse>;

public record SaleItemDto(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    Guid? ProductVariantId = null,
    decimal DiscountPercent = 0,
    decimal TaxRate = 0);

public record SaleResponse(
    Guid Id,
    string InvoiceNumber,
    Guid WarehouseId,
    string WarehouseName,
    Guid? CustomerId,
    string? CustomerName,
    SaleStatus Status,
    decimal SubTotal,
    decimal TaxAmount,
    decimal DiscountAmount,
    decimal ShippingAmount,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal ChangeAmount,
    decimal OutstandingAmount,
    int ItemCount,
    DateTime SaleDate);

public class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("Warehouse ID is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be non-negative");
        });
    }
}

public class CreateSaleCommandHandler : ICommandHandler<CreateSaleCommand, SaleResponse>
{
    private readonly IRepository<Sale> _saleRepository;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IRepository<StockLevel> _stockLevelRepository;
    private readonly IRepository<InventoryTransaction> _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public CreateSaleCommandHandler(
        IRepository<Sale> saleRepository,
        IRepository<Product> productRepository,
        IRepository<Customer> customerRepository,
        IRepository<Warehouse> warehouseRepository,
        IRepository<StockLevel> stockLevelRepository,
        IRepository<InventoryTransaction> transactionRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider,
        ICurrentUserService currentUserService,
        IDateTime dateTime)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _warehouseRepository = warehouseRepository;
        _stockLevelRepository = stockLevelRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public async Task<Result<SaleResponse>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (!tenantId.HasValue)
        {
            return Result.Failure<SaleResponse>(
                Error.Unauthorized("Tenant context is required."));
        }

        var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId, cancellationToken);
        if (warehouse == null)
        {
            return Result.Failure<SaleResponse>(
                Error.NotFound("Warehouse", request.WarehouseId));
        }

        Customer? customer = null;
        if (request.CustomerId.HasValue)
        {
            customer = await _customerRepository.GetByIdAsync(request.CustomerId.Value, cancellationToken);
            if (customer == null)
            {
                return Result.Failure<SaleResponse>(
                    Error.NotFound("Customer", request.CustomerId.Value));
            }
        }

        var invoiceNumber = $"INV-{_dateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var sale = new Sale
        {
            TenantId = tenantId.Value,
            InvoiceNumber = invoiceNumber,
            WarehouseId = request.WarehouseId,
            CustomerId = request.CustomerId,
            Status = SaleStatus.Completed,
            CashierId = _currentUserService.UserId ?? Guid.Empty,
            SaleDate = _dateTime.UtcNow,
            Notes = request.Notes,
            CouponCode = request.CouponCode,
            ShippingAmount = request.ShippingAmount
        };

        decimal subTotal = 0;
        decimal totalTax = 0;
        decimal itemDiscounts = 0;

        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null)
            {
                return Result.Failure<SaleResponse>(
                    Error.NotFound("Product", item.ProductId));
            }

            // Check stock if product tracks inventory
            if (product.TrackInventory)
            {
                var stockLevel = await _stockLevelRepository.FirstOrDefaultAsync(
                    sl => sl.ProductId == item.ProductId &&
                          sl.WarehouseId == request.WarehouseId &&
                          sl.ProductVariantId == item.ProductVariantId,
                    cancellationToken);

                var availableQty = stockLevel?.AvailableQuantity ?? 0;
                if (availableQty < item.Quantity && !product.AllowNegativeStock)
                {
                    return Result.Failure<SaleResponse>(
                        Error.Validation("Sale.InsufficientStock",
                            $"Insufficient stock for product '{product.Name}'. Available: {availableQty}, Requested: {item.Quantity}"));
                }

                // Deduct stock
                if (stockLevel != null)
                {
                    stockLevel.Quantity -= item.Quantity;
                    stockLevel.LastUpdated = _dateTime.UtcNow;
                    _stockLevelRepository.Update(stockLevel);

                    // Create inventory transaction
                    await _transactionRepository.AddAsync(new InventoryTransaction
                    {
                        TenantId = tenantId.Value,
                        ProductId = item.ProductId,
                        ProductVariantId = item.ProductVariantId,
                        WarehouseId = request.WarehouseId,
                        Type = InventoryTransactionType.Sale,
                        Quantity = -item.Quantity,
                        UnitCost = product.CostPrice,
                        TotalCost = product.CostPrice * item.Quantity,
                        RunningBalance = stockLevel.Quantity,
                        ReferenceType = nameof(Sale),
                        ReferenceId = sale.Id,
                        Notes = $"Sale: {invoiceNumber}"
                    }, cancellationToken);
                }
            }

            var itemDiscount = item.UnitPrice * item.Quantity * (item.DiscountPercent / 100);
            var lineSubTotal = (item.UnitPrice * item.Quantity) - itemDiscount;
            var lineTax = lineSubTotal * (item.TaxRate / 100);
            var lineTotal = lineSubTotal + lineTax;

            sale.Items.Add(new SaleItem
            {
                ProductId = item.ProductId,
                ProductVariantId = item.ProductVariantId,
                ProductName = product.Name,
                SKU = product.SKU,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                CostPrice = product.CostPrice,
                DiscountPercent = item.DiscountPercent,
                DiscountAmount = itemDiscount,
                TaxRate = item.TaxRate,
                TaxAmount = lineTax,
                LineTotal = lineTotal
            });

            subTotal += item.UnitPrice * item.Quantity;
            totalTax += lineTax;
            itemDiscounts += itemDiscount;
        }

        sale.SubTotal = subTotal;
        sale.TaxAmount = totalTax;
        sale.DiscountAmount = itemDiscounts + request.DiscountAmount;
        sale.TotalAmount = subTotal - sale.DiscountAmount + totalTax + request.ShippingAmount;
        sale.PaidAmount = request.PaidAmount;
        sale.ChangeAmount = Math.Max(0, request.PaidAmount - sale.TotalAmount);

        // Update customer stats if customer exists
        if (customer != null)
        {
            customer.TotalOrders++;
            customer.TotalPurchases += sale.TotalAmount;
            _customerRepository.Update(customer);
        }

        await _saleRepository.AddAsync(sale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new SaleResponse(
            sale.Id,
            sale.InvoiceNumber,
            sale.WarehouseId,
            warehouse.Name,
            sale.CustomerId,
            customer?.Name,
            sale.Status,
            sale.SubTotal,
            sale.TaxAmount,
            sale.DiscountAmount,
            sale.ShippingAmount,
            sale.TotalAmount,
            sale.PaidAmount,
            sale.ChangeAmount,
            sale.OutstandingAmount,
            sale.Items.Count,
            sale.SaleDate));
    }
}
