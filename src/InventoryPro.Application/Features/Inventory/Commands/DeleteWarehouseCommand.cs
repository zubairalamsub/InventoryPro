using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Inventory.Commands;

public record DeleteWarehouseCommand(Guid Id) : ICommand;

public class DeleteWarehouseCommandValidator : AbstractValidator<DeleteWarehouseCommand>
{
    public DeleteWarehouseCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse ID is required");
    }
}

public class DeleteWarehouseCommandHandler : ICommandHandler<DeleteWarehouseCommand>
{
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IRepository<StockLevel> _stockLevelRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWarehouseCommandHandler(
        IRepository<Warehouse> warehouseRepository,
        IRepository<StockLevel> stockLevelRepository,
        IUnitOfWork unitOfWork)
    {
        _warehouseRepository = warehouseRepository;
        _stockLevelRepository = stockLevelRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse == null)
        {
            return Result.Failure(
                Error.NotFound("Warehouse", request.Id));
        }

        // Check if warehouse has any stock
        var hasStock = await _stockLevelRepository.AnyAsync(
            sl => sl.WarehouseId == request.Id && sl.Quantity > 0,
            cancellationToken);

        if (hasStock)
        {
            return Result.Failure(
                Error.Validation("Warehouse.HasStock", "Cannot delete warehouse with existing stock. Please transfer or adjust stock first."));
        }

        _warehouseRepository.Remove(warehouse);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
