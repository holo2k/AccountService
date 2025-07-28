using FluentValidation;

namespace AccountService.Features.Account.GetAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class GetAccountQueryValidator : AbstractValidator<GetAccountQuery>
{
    public GetAccountQueryValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Идентификатор счета обязателен");
    }
}