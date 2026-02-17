using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Customers.Commands;

public record DeleteCustomerCommand(Guid Id) : ICommand;

public class DeleteCustomerCommandValidator : AbstractValidator<DeleteCustomerCommand>
{
    public DeleteCustomerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Customer ID is required");
    }
}

public class DeleteCustomerCommandHandler : ICommandHandler<DeleteCustomerCommand>
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Sale> _saleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomerCommandHandler(
        IRepository<Customer> customerRepository,
        IRepository<Sale> saleRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _saleRepository = saleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer == null)
        {
            return Result.Failure(
                Error.NotFound("Customer", request.Id));
        }

        // Check if customer has any sales
        var hasSales = await _saleRepository.AnyAsync(
            s => s.CustomerId == request.Id,
            cancellationToken);

        if (hasSales)
        {
            return Result.Failure(
                Error.Validation("Customer.HasSales", "Cannot delete customer with existing sales. Consider deactivating instead."));
        }

        _customerRepository.Remove(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
