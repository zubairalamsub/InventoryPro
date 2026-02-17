using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Common.ValueObjects;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Inventory.Commands;

public record UpdateWarehouseCommand(
    Guid Id,
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
    bool IsActive = true
) : ICommand<WarehouseResponse>;

public class UpdateWarehouseCommandValidator : AbstractValidator<UpdateWarehouseCommand>
{
    public UpdateWarehouseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse ID is required");

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

public class UpdateWarehouseCommandHandler : ICommandHandler<UpdateWarehouseCommand, WarehouseResponse>
{
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantProvider _tenantProvider;

    public UpdateWarehouseCommandHandler(
        IRepository<Warehouse> warehouseRepository,
        IUnitOfWork unitOfWork,
        ITenantProvider tenantProvider)
    {
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
        _tenantProvider = tenantProvider;
    }

    public async Task<Result<WarehouseResponse>> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse == null)
        {
            return Result.Failure<WarehouseResponse>(
                Error.NotFound("Warehouse", request.Id));
        }

        var tenantId = _tenantProvider.GetTenantId();

        // Check for duplicate code within tenant (excluding current)
        if (!string.IsNullOrEmpty(request.Code))
        {
            var existingWarehouse = await _warehouseRepository.FirstOrDefaultAsync(
                w => w.Code == request.Code && w.TenantId == tenantId && w.Id != request.Id,
                cancellationToken);

            if (existingWarehouse != null)
            {
                return Result.Failure<WarehouseResponse>(
                    Error.Conflict("Warehouse.DuplicateCode", $"A warehouse with code '{request.Code}' already exists."));
            }
        }

        // If this is marked as default, unset other defaults
        if (request.IsDefault && !warehouse.IsDefault)
        {
            var currentDefault = await _warehouseRepository.FirstOrDefaultAsync(
                w => w.IsDefault && w.TenantId == tenantId && w.Id != request.Id,
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

        warehouse.Name = request.Name;
        warehouse.Code = request.Code;
        warehouse.Address = address;
        warehouse.ManagerId = request.ManagerId;
        warehouse.Phone = request.Phone;
        warehouse.Email = request.Email;
        warehouse.IsDefault = request.IsDefault;
        warehouse.IsActive = request.IsActive;

        _warehouseRepository.Update(warehouse);
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
