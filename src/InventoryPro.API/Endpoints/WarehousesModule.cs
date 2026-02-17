using Carter;
using InventoryPro.Application.Features.Inventory.Commands;
using InventoryPro.Application.Features.Inventory.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryPro.API.Endpoints;

public class WarehousesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/warehouses")
            .WithTags("Warehouses")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetWarehouses)
            .WithName("GetWarehouses")
            .WithSummary("Get all warehouses with pagination and filtering");

        group.MapGet("/{id:guid}", GetWarehouseById)
            .WithName("GetWarehouseById")
            .WithSummary("Get a warehouse by ID");

        group.MapPost("/", CreateWarehouse)
            .WithName("CreateWarehouse")
            .WithSummary("Create a new warehouse");

        group.MapPut("/{id:guid}", UpdateWarehouse)
            .WithName("UpdateWarehouse")
            .WithSummary("Update an existing warehouse");

        group.MapDelete("/{id:guid}", DeleteWarehouse)
            .WithName("DeleteWarehouse")
            .WithSummary("Delete a warehouse");
    }

    private static async Task<IResult> GetWarehouses(
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetWarehousesQuery(
            searchTerm,
            isActive,
            pageNumber > 0 ? pageNumber : 1,
            pageSize > 0 ? pageSize : 10);

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> GetWarehouseById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetWarehouseByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 404);
    }

    private static async Task<IResult> CreateWarehouse(
        [FromBody] CreateWarehouseRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateWarehouseCommand(
            request.Name,
            request.Code,
            request.Street,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.ManagerId,
            request.Phone,
            request.Email,
            request.IsDefault);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/warehouses/{result.Value.Id}", result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> UpdateWarehouse(
        Guid id,
        [FromBody] UpdateWarehouseRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateWarehouseCommand(
            id,
            request.Name,
            request.Code,
            request.Street,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.ManagerId,
            request.Phone,
            request.Email,
            request.IsDefault,
            request.IsActive);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> DeleteWarehouse(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new DeleteWarehouseCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }
}

public record CreateWarehouseRequest(
    string Name,
    string? Code = null,
    string? Street = null,
    string? City = null,
    string? State = null,
    string? PostalCode = null,
    string? Country = null,
    Guid? ManagerId = null,
    string? Phone = null,
    string? Email = null,
    bool IsDefault = false);

public record UpdateWarehouseRequest(
    string Name,
    string? Code = null,
    string? Street = null,
    string? City = null,
    string? State = null,
    string? PostalCode = null,
    string? Country = null,
    Guid? ManagerId = null,
    string? Phone = null,
    string? Email = null,
    bool IsDefault = false,
    bool IsActive = true);
