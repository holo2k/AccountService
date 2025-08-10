using AccountService.Features.Account.GetAccount;
using AccountService.PipelineBehaviors;
using AccountService.UserService.Abstractions;
using MediatR;

namespace AccountService.Features.Account.CheckAccountOwnership;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class CheckAccountOwnershipHandler : IRequestHandler<CheckAccountOwnershipQuery, MbResult<bool>>
{
    private readonly IMediator _mediator;
    private readonly IUserService _userService;

    public CheckAccountOwnershipHandler(IUserService userService, IMediator mediator)
    {
        _userService = userService;
        _mediator = mediator;
    }

    public async Task<MbResult<bool>> Handle(CheckAccountOwnershipQuery request, CancellationToken cancellationToken)
    {
        if (!await _userService.IsExistsAsync(request.OwnerId))
            return MbResult<bool>.Fail(new MbError
            {
                Code = "NotFound",
                Message = "Владелец не найден."
            });

        var accountResult = await _mediator.Send(new GetAccountQuery(request.AccountId), cancellationToken);

        if (accountResult.Result is null)
            return MbResult<bool>.Fail(new MbError
            {
                Code = "NotFound",
                Message = "Счёт не найден."
            });

        if (accountResult.Result.OwnerId != request.OwnerId)
            return MbResult<bool>.Fail(new MbError
            {
                Code = "OwnershipMismatch",
                Message = "Счёт не принадлежит владельцу."
            });

        return MbResult<bool>.Success(true);
    }
}