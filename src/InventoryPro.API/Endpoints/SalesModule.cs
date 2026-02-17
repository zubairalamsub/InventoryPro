using Carter;
using InventoryPro.Application.Features.Sales.Commands;
using InventoryPro.Application.Features.Sales.Queries;
using InventoryPro.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryPro.API.Endpoints;

public class SalesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sales")
            .WithTags("Sales")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetSales)
            .WithName("GetSales")
            .WithSummary("Get all sales with pagination and filtering");

        group.MapGet("/{id:guid}", GetSaleById)
            .WithName("GetSaleById")
            .WithSummary("Get a sale by ID");

        group.MapPost("/", CreateSale)
            .WithName("CreateSale")
            .WithSummary("Create a new sale");

        group.MapPost("/{id:guid}/void", VoidSale)
            .WithName("VoidSale")
            .WithSummary("Void a sale");
    }

    private static async Task<IResult> GetSales(
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] SaleStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetSalesQuery(
            searchTerm,
            customerId,
            warehouseId,
            status,
            fromDate,
            toDate,
            pageNumber > 0 ? pageNumber : 1,
            pageSize > 0 ? pageSize : 10);

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> GetSaleById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetSaleByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 404);
    }

    private static async Task<IResult> CreateSale(
        [FromBody] CreateSaleRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateSaleCommand(
            request.WarehouseId,
            request.Items.Select(i => new SaleItemDto(
                i.ProductId,
                i.Quantity,
                i.UnitPrice,
                i.ProductVariantId,
                i.DiscountPercent,
                i.TaxRate)).ToList(),
            request.CustomerId,
            request.DiscountAmount,
            request.ShippingAmount,
            request.PaidAmount,
            request.Notes,
            request.CouponCode);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/sales/{result.Value.Id}", result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> VoidSale(
        Guid id,
        [FromBody] VoidSaleRequest? request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new VoidSaleCommand(id, request?.Reason);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }
}

public record CreateSaleRequest(
    Guid WarehouseId,
    List<SaleItemRequest> Items,
    Guid? CustomerId = null,
    decimal DiscountAmount = 0,
    decimal ShippingAmount = 0,
    decimal PaidAmount = 0,
    string? Notes = null,
    string? CouponCode = null);

public record SaleItemRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    Guid? ProductVariantId = null,
    decimal DiscountPercent = 0,
    decimal TaxRate = 0);

public record VoidSaleRequest(string? Reason);
