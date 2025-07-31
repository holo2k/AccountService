using FluentValidation;

namespace AccountService.Features.Account.GetAccountBalance;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class GetAccountBalanceQueryValidator : AbstractValidator<GetAccountBalanceQuery>
{
    public GetAccountBalanceQueryValidator()
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("OwnerId обязателен.");
    }
}