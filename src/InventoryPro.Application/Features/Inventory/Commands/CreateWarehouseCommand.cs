using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Common.ValueObjects;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Inventory.Commands;

public record CreateWarehouseCommand(
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
    bool IsDefault = false
) : ICommand<WarehouseResponse>;

public record WarehouseResponse(
    Guid Id,
    string Name,
    string? Code,
    string? Phone,
    string? Email,
    bool IsDefault,
    bool IsActive,
    DateTime CreatedAt);

public class CreateWarehouseCommandValidator : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Warehouse name is required")
            .MaximumLength(200).WithMessage("Warehouse name cannot exceed 200 characters");

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Warehouse code cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Code));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email address")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class CreateWarehouseCommandHandler : ICommandHandler<CreateWarehouseCommand, WarehouseResponse>
{
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;

    public CreateWarehouseCommandHandler(
        IRepository<Warehouse> warehouseRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider)
    {
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
    }

    public async Task<Result<WarehouseResponse>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantProvider.GetTenantId();
        if (!tenantId.HasValue)
        {
            return Result.Failure<WarehouseResponse>(
                Error.Unauthorized("Tenant context is required."));
        }

        // Check for duplicate code within tenant
        if (!string.IsNullOrEmpty(request.Code))
        {
            var existingWarehouse = await _warehouseRepository.FirstOrDefaultAsync(
                w => w.Code == request.Code && w.TenantId == tenantId.Value,
                cancellationToken);

            if (existingWarehouse != null)
            {
                return Result.Failure<WarehouseResponse>(
                    Error.Conflict("Warehouse.DuplicateCode", $"A warehouse with code '{request.Code}' already exists."));
            }
        }

        // If this is marked as default, unset other defaults
        if (request.IsDefault)
        {
            var currentDefault = await _warehouseRepository.FirstOrDefaultAsync(
                w => w.IsDefault && w.TenantId == tenantId.Value,
                cancellationToken);

            if (currentDefault != null)
            {
                currentDefault.IsDefault = false;
                _warehouseRepository.Update(currentDefault);
            }
        }

        Address? address = null;
        if (!string.IsNullOrEmpty(request.Street) || !string.IsNullOrEmpty(request.City))
        {
            address = new Address(
                request.Street,
                null, // Street2
                request.City,
                request.State,
                request.PostalCode,
                request.Country);
        }

        var warehouse = new Warehouse
        {
            TenantId = tenantId.Value,
            Name = request.Name,
            Code = request.Code,
            Address = address,
            ManagerId = request.ManagerId,
            Phone = request.Phone,
            Email = request.Email,
            IsDefault = request.IsDefault,
            IsActive = true
        };

        await _warehouseRepository.AddAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new WarehouseResponse(
            warehouse.Id,
            warehouse.Name,
            warehouse.Code,
            warehouse.Phone,
            warehouse.Email,
            warehouse.IsDefault,
            warehouse.IsActive,
            warehouse.CreatedAt));
    }
}
