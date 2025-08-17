using FluentValidation;

namespace AccountService.Features.Account.FreezeAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class FreezeAccountCommandValidator : AbstractValidator<FreezeAccountCommand>
{
    public FreezeAccountCommandValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty().WithMessage("AccountId не может быть пустым.");
    }
}