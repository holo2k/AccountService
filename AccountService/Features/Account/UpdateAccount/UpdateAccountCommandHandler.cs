using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.PipelineBehaviors;
using AutoMapper;
using MediatR;

namespace AccountService.Features.Account.UpdateAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, MbResult<Guid>>
{
    private readonly IMapper _mapper;
    private readonly IAccountRepository _repository;

    public UpdateAccountCommandHandler(IAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MbResult<Guid>> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var existingAccount = await _repository.GetByIdAsync(request.Account.Id);

        if (existingAccount == null)
            return MbResult<Guid>.Fail(new MbError { Code = "NotFound", Message = "Account not found" });

        _mapper.Map(request.Account, existingAccount);

        var result = await _repository.UpdateAsync(existingAccount);

        return !result.IsSuccess ? MbResult<Guid>.Fail(result.Error!) : MbResult<Guid>.Success(existingAccount.Id);
    }
}