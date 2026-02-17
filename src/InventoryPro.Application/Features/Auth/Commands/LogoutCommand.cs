using FluentValidation;
using InventoryPro.Application.Common.Interfaces;
using InventoryPro.Application.Common.Models;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;

namespace InventoryPro.Application.Features.Auth.Commands;

public record LogoutCommand(string RefreshToken) : ICommand;

public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}

public class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly IRepository<RefreshToken> _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTime _dateTime;

    public LogoutCommandHandler(
        IRepository<RefreshToken> refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IDateTime dateTime)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _refreshTokenRepository.FirstOrDefaultAsync(
            t => t.Token == request.RefreshToken, cancellationToken);

        if (storedToken != null && !storedToken.IsRevoked)
        {
            storedToken.RevokedAt = _dateTime.UtcNow;
            _refreshTokenRepository.Update(storedToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
