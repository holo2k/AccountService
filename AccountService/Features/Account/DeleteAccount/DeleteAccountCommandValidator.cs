using FluentValidation;

namespace AccountService.Features.Account.DeleteAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("ID счёта обязательно для заполнения.");
    }
}