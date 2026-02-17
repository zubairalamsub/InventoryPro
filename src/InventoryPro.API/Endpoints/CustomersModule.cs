using Carter;
using InventoryPro.Application.Features.Customers.Commands;
using InventoryPro.Application.Features.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryPro.API.Endpoints;

public class CustomersModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers")
            .WithTags("Customers")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetCustomers)
            .WithName("GetCustomers")
            .WithSummary("Get all customers with pagination and filtering");

        group.MapGet("/{id:guid}", GetCustomerById)
            .WithName("GetCustomerById")
            .WithSummary("Get a customer by ID");

        group.MapPost("/", CreateCustomer)
            .WithName("CreateCustomer")
            .WithSummary("Create a new customer");

        group.MapPut("/{id:guid}", UpdateCustomer)
            .WithName("UpdateCustomer")
            .WithSummary("Update an existing customer");

        group.MapDelete("/{id:guid}", DeleteCustomer)
            .WithName("DeleteCustomer")
            .WithSummary("Delete a customer");
    }

    private static async Task<IResult> GetCustomers(
        [FromQuery] string? searchTerm,
        [FromQuery] Guid? customerGroupId,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomersQuery(
            searchTerm,
            customerGroupId,
            isActive,
            pageNumber > 0 ? pageNumber : 1,
            pageSize > 0 ? pageSize : 10);

        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> GetCustomerById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetCustomerByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 404);
    }

    private static async Task<IResult> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand(
            request.Name,
            request.CustomerGroupId,
            request.Code,
            request.Email,
            request.Phone,
            request.AlternatePhone,
            request.Street,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.TaxIdentificationNo,
            request.CreditLimit,
            request.DateOfBirth,
            request.Notes,
            request.Tags);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/customers/{result.Value.Id}", result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> UpdateCustomer(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.Name,
            request.CustomerGroupId,
            request.Code,
            request.Email,
            request.Phone,
            request.AlternatePhone,
            request.Street,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.TaxIdentificationNo,
            request.CreditLimit,
            request.DateOfBirth,
            request.Notes,
            request.Tags,
            request.IsActive);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }

    private static async Task<IResult> DeleteCustomer(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new DeleteCustomerCommand(id);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.Problem(detail: result.Error.Message, statusCode: 400);
    }
}

public record CreateCustomerRequest(
    string Name,
    Guid? CustomerGroupId = null,
    string? Code = null,
    string? Email = null,
    string? Phone = null,
    string? AlternatePhone = null,
    string? Street = null,
    string? City = null,
    string? State = null,
    string? PostalCode = null,
    string? Country = null,
    string? TaxIdentificationNo = null,
    decimal? CreditLimit = null,
    DateOnly? DateOfBirth = null,
    string? Notes = null,
    string[]? Tags = null);

public record UpdateCustomerRequest(
    string Name,
    Guid? CustomerGroupId = null,
    string? Code = null,
    string? Email = null,
    string? Phone = null,
    string? AlternatePhone = null,
    string? Street = null,
    string? City = null,
    string? State = null,
    string? PostalCode = null,
    string? Country = null,
    string? TaxIdentificationNo = null,
    decimal? CreditLimit = null,
    DateOnly? DateOfBirth = null,
    string? Notes = null,
    string[]? Tags = null,
    bool IsActive = true);
