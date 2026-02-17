using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Inventory.Queries;

public record GetWarehouseByIdQuery(Guid Id) : IQuery<WarehouseDetailResponse>;

public record WarehouseDetailResponse(
    Guid Id,
    string Name,
    string? Code,
    string? Street,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    Guid? ManagerId,
    string? ManagerName,
    string? Phone,
    string? Email,
    bool IsDefault,
    bool IsActive,
    int TotalProducts,
    int TotalQuantity,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class GetWarehouseByIdQueryValidator : AbstractValidator<GetWarehouseByIdQuery>
{
    public GetWarehouseByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Warehouse ID is required");
    }
}

public class GetWarehouseByIdQueryHandler : IQueryHandler<GetWarehouseByIdQuery, WarehouseDetailResponse>
{
    private readonly IRepository<Warehouse> _warehouseRepository;
    private readonly IRepository<StockLevel> _stockLevelRepository;

    public GetWarehouseByIdQueryHandler(
        IRepository<Warehouse> warehouseRepository,
        IRepository<StockLevel> stockLevelRepository)
    {
        _warehouseRepository = warehouseRepository;
        _stockLevelRepository = stockLevelRepository;
    }

    public async Task<Result<WarehouseDetailResponse>> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse == null)
        {
            return Result.Failure<WarehouseDetailResponse>(
                Error.NotFound("Warehouse", request.Id));
        }

        var stockLevels = await _stockLevelRepository.GetAllAsync(cancellationToken);
        var warehouseStock = stockLevels.Where(sl => sl.WarehouseId == request.Id && sl.Quantity > 0).ToList();

        return Result.Success(new WarehouseDetailResponse(
            warehouse.Id,
            warehouse.Name,
            warehouse.Code,
            warehouse.Address?.Street,
            warehouse.Address?.City,
            warehouse.Address?.State,
            warehouse.Address?.PostalCode,
            warehouse.Address?.Country,
            warehouse.ManagerId,
            warehouse.Manager?.FullName,
            warehouse.Phone,
            warehouse.Email,
            warehouse.IsDefault,
            warehouse.IsActive,
            warehouseStock.Select(sl => sl.ProductId).Distinct().Count(),
            warehouseStock.Sum(sl => sl.Quantity),
            warehouse.CreatedAt,
            warehouse.UpdatedAt));
    }
}
