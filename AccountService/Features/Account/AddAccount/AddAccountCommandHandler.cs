using AccountService.Features.Outbox.Service;
using AccountService.Infrastructure.Repository;
using AccountService.PipelineBehaviors;
using AutoMapper;
using MediatR;

namespace AccountService.Features.Account.AddAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class AddAccountCommandHandler : IRequestHandler<AddAccountCommand, MbResult<Guid>>
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IOutboxService _outboxService;

    public AddAccountCommandHandler(IMapper mapper, AppDbContext db, IOutboxService outboxService)
    {
        _mapper = mapper;
        _db = db;
        _outboxService = outboxService;
    }

    public async Task<MbResult<Guid>> Handle(AddAccountCommand request, CancellationToken cancellationToken)
    {
        var account = _mapper.Map<Account>(request.Account);
        account.Id = Guid.CreateVersion7();
        account.OpenDate = DateTime.UtcNow;
        account.IsFrozen = false;

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await _db.Accounts.AddAsync(account, cancellationToken);

            await _outboxService.AddAccountOpenedEventAsync(account);

            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return MbResult<Guid>.Success(account.Id);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(cancellationToken);
            return MbResult<Guid>.Fail(new MbError { Code = "AddAccountError", Message = ex.Message });
        }
    }
}