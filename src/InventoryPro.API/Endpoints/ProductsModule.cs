using Carter;
using InventoryPro.Application.Features.Products.Commands;
using InventoryPro.Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryPro.API.Endpoints;

public class ProductsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetProducts)
            .WithName("GetProducts")
            .WithSummary("Get all products with pagination and filtering");

        group.MapGet("/{id:guid}", GetProductById)
            .WithName("GetProductById")
            .WithSummary("Get a product by ID");

        group.MapPost("/", CreateProduct)
            .WithName("CreateProduct")
            .WithSummary("Create a new product");

        group.MapPut("/{id:guid}", UpdateProduct)
            .WithName("UpdateProduct")
            .WithSummary("Update an existing product");

        group.MapDelete("/{id:guid}", DeleteProduct)
            .WithName("DeleteProduct")
            .WithSummary("Delete a product");
    }

    private static async Task<IResult> GetProducts(
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? categoryId,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetProductsQuery(
            searchTerm,
            categoryId,
            isActive,
            pageNumber > 0 ? pageNumber : 1,
            pageSize > 0 ? pageSize : 10);

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> GetProductById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 404);
    }

    private static async Task<IResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.SKU,
            request.CostPrice,
            request.SellingPrice,
            request.CategoryId,
            request.UnitOfMeasureId,
            request.Barcode,
            request.Description,
            request.ShortDescription,
            request.WholesalePrice,
            request.MinimumPrice,
            request.ReorderLevel,
            request.ReorderQuantity,
            request.MaxStockLevel,
            request.Weight,
            request.Dimensions,
            request.Tags,
            request.IsService,
            request.TrackInventory,
            request.AllowNegativeStock);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/products/{result.Value.Id}", result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.SKU,
            request.CostPrice,
            request.SellingPrice,
            request.CategoryId,
            request.UnitOfMeasureId,
            request.Barcode,
            request.Description,
            request.ShortDescription,
            request.WholesalePrice,
            request.MinimumPrice,
            request.ReorderLevel,
            request.ReorderQuantity,
            request.MaxStockLevel,
            request.Weight,
            request.Dimensions,
            request.Tags,
            request.IsActive,
            request.IsService,
            request.TrackInventory,
            request.AllowNegativeStock);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> DeleteProduct(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }
}

public record CreateProductRequest(
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
    bool AllowNegativeStock = false);

public record UpdateProductRequest(
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
    bool IsActive = true,
    bool IsService = false,
    bool TrackInventory = true,
    bool AllowNegativeStock = false);
