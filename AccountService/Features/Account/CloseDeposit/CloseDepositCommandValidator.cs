using FluentValidation;

namespace AccountService.Features.Account.CloseDeposit;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class CloseDepositCommandValidator : AbstractValidator<CloseDepositCommand>
{
    public CloseDepositCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("AccountId не должен быть пустым");
    }
}