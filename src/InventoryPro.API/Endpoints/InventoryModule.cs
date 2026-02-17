using Carter;
using InventoryPro.Application.Features.Inventory.Commands;
using InventoryPro.Application.Features.Inventory.Queries;
using InventoryPro.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryPro.API.Endpoints;

public class InventoryModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory")
            .WithTags("Inventory")
            .WithOpenApi()
            .RequireAuthorization();

        // Stock Levels
        group.MapGet("/stock-levels", GetStockLevels)
            .WithName("GetStockLevels")
            .WithSummary("Get stock levels with pagination and filtering");

        // Transactions
        group.MapGet("/transactions", GetTransactions)
            .WithName("GetInventoryTransactions")
            .WithSummary("Get inventory transactions with pagination and filtering");

        // Stock Adjustments
        group.MapPost("/adjustments", CreateStockAdjustment)
            .WithName("CreateStockAdjustment")
            .WithSummary("Create a stock adjustment");

        // Stock Transfers
        group.MapPost("/transfers", CreateStockTransfer)
            .WithName("CreateStockTransfer")
            .WithSummary("Create a stock transfer between warehouses");

        group.MapPost("/transfers/{id:guid}/complete", CompleteStockTransfer)
            .WithName("CompleteStockTransfer")
            .WithSummary("Complete a pending stock transfer");
    }

    private static async Task<IResult> GetStockLevels(
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId,
        [FromQuery] bool? lowStockOnly,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetStockLevelsQuery(
            warehouseId,
            productId,
            lowStockOnly,
            pageNumber > 0 ? pageNumber : 1,
            pageSize > 0 ? pageSize : 10);

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> GetTransactions(
        [FromQuery] Guid? productId,
        [FromQuery] Guid? warehouseId,
        [FromQuery] InventoryTransactionType? type,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetInventoryTransactionsQuery(
            productId,
            warehouseId,
            type,
            fromDate,
            toDate,
            pageNumber > 0 ? pageNumber : 1,
            pageSize > 0 ? pageSize : 20);

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> CreateStockAdjustment(
        [FromBody] CreateStockAdjustmentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateStockAdjustmentCommand(
            request.WarehouseId,
            request.Reason,
            request.Items.Select(i => new StockAdjustmentItemDto(
                i.ProductId,
                i.QuantityAdjusted,
                i.ProductVariantId,
                i.Notes)).ToList(),
            request.Notes);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/inventory/adjustments/{result.Value.Id}", result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> CreateStockTransfer(
        [FromBody] CreateStockTransferRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateStockTransferCommand(
            request.FromWarehouseId,
            request.ToWarehouseId,
            request.Items.Select(i => new StockTransferItemDto(
                i.ProductId,
                i.Quantity,
                i.ProductVariantId,
                i.Notes)).ToList(),
            request.Notes);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/inventory/transfers/{result.Value.Id}", result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> CompleteStockTransfer(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CompleteStockTransferCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }
}

public record CreateStockAdjustmentRequest(
    Guid WarehouseId,
    AdjustmentReason Reason,
    List<StockAdjustmentItemRequest> Items,
    string? Notes = null);

public record StockAdjustmentItemRequest(
    Guid ProductId,
    int QuantityAdjusted,
    Guid? ProductVariantId = null,
    string? Notes = null);

public record CreateStockTransferRequest(
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    List<StockTransferItemRequest> Items,
    string? Notes = null);

public record StockTransferItemRequest(
    Guid ProductId,
    int Quantity,
    Guid? ProductVariantId = null,
    string? Notes = null);
