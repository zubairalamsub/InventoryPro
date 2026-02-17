using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Sales.Queries;

public record GetSaleByIdQuery(Guid Id) : IQuery<SaleDetailResponse>;

public record SaleDetailResponse(
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
    string? Notes,
    Guid CashierId,
    string? CashierName,
    DateTime SaleDate,
    DateTime CreatedAt,
    List<SaleItemDetailResponse> Items);

public record SaleItemDetailResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string SKU,
    int Quantity,
    decimal UnitPrice,
    decimal CostPrice,
    decimal DiscountPercent,
    decimal DiscountAmount,
    decimal TaxRate,
    decimal TaxAmount,
    decimal LineTotal,
    decimal Profit);

public class GetSaleByIdQueryValidator : AbstractValidator<GetSaleByIdQuery>
{
    public GetSaleByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Sale ID is required");
    }
}

public class GetSaleByIdQueryHandler : IQueryHandler<GetSaleByIdQuery, SaleDetailResponse>
{
    private readonly IRepository<Sale> _saleRepository;

    public GetSaleByIdQueryHandler(IRepository<Sale> saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<Result<SaleDetailResponse>> Handle(GetSaleByIdQuery request, CancellationToken cancellationToken)
    {
        var sales = await _saleRepository.GetAllAsync(cancellationToken);
        var sale = sales.FirstOrDefault(s => s.Id == request.Id);

        if (sale == null)
        {
            return Result.Failure<SaleDetailResponse>(
                Error.NotFound("Sale", request.Id));
        }

        var items = sale.Items?.Select(i => new SaleItemDetailResponse(
            i.Id,
            i.ProductId,
            i.ProductName,
            i.SKU,
            i.Quantity,
            i.UnitPrice,
            i.CostPrice,
            i.DiscountPercent,
            i.DiscountAmount,
            i.TaxRate,
            i.TaxAmount,
            i.LineTotal,
            i.Profit)).ToList() ?? new List<SaleItemDetailResponse>();

        return Result.Success(new SaleDetailResponse(
            sale.Id,
            sale.InvoiceNumber,
            sale.WarehouseId,
            sale.Warehouse?.Name ?? "Unknown",
            sale.CustomerId,
            sale.Customer?.Name,
            sale.Status,
            sale.SubTotal,
            sale.TaxAmount,
            sale.DiscountAmount,
            sale.ShippingAmount,
            sale.TotalAmount,
            sale.PaidAmount,
            sale.ChangeAmount,
            sale.OutstandingAmount,
            sale.Notes,
            sale.CashierId,
            sale.Cashier?.FullName,
            sale.SaleDate,
            sale.CreatedAt,
            items));
    }
}
