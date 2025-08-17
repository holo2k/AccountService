using AccountService.Features.Account.GetAccountsByOwnerId;
using AccountService.Features.Outbox.Service;
using AccountService.Infrastructure.Repository;
using AccountService.PipelineBehaviors;
using MediatR;

namespace AccountService.Features.Account.FreezeAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class FreezeAccountCommandHandler : IRequestHandler<FreezeAccountCommand, MbResult<Guid>>
{
    private readonly AppDbContext _db;
    private readonly IMediator _mediator;
    private readonly IOutboxService _outbox;

    public FreezeAccountCommandHandler(IOutboxService outbox, IMediator mediator, AppDbContext db)
    {
        _outbox = outbox;
        _mediator = mediator;
        _db = db;
    }

    public async Task<MbResult<Guid>> Handle(FreezeAccountCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var accountsResult =
                await _mediator.Send(new GetAccountsByOwnerIdQuery(request.ClientId), cancellationToken);

            if (!accountsResult.IsSuccess)
                return MbResult<Guid>.Fail(new MbError
                    { Code = "AccountsFetchError", Message = "Не удалось получить счета клиента." });

            var accounts = accountsResult.Result!;

            var anyFrozen = accounts.Any(a => a.IsFrozen);

            switch (anyFrozen)
            {
                case true when request.IsFrozen:
                    return MbResult<Guid>.Fail(new MbError
                        { Code = "AlreadyBlocked", Message = "Клиент уже заблокирован" });
                case false when !request.IsFrozen:
                    return MbResult<Guid>.Fail(new MbError
                        { Code = "AlreadyUnblocked", Message = "Клиент уже разблокирован" });
            }

            var type = request.IsFrozen ? "ClientBlocked" : "ClientUnblocked";
            await _outbox.AddFreezeUnfreezeClientEvent(request.ClientId, type);
            await _db.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return MbResult<Guid>.Success(request.ClientId);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}