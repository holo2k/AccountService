using FluentValidation;

namespace AccountService.Features.Transaction.TransferBetweenAccounts;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class TransferBetweenAccountsCommandValidator : AbstractValidator<TransferBetweenAccountsCommand>
{
    public TransferBetweenAccountsCommandValidator()
    {
        RuleFor(x => x.PayloadModel.FromAccountId)
            .NotEmpty().WithMessage("Счёт отправителя обязателен для заполнения.");

        RuleFor(x => x.PayloadModel.ToAccountId)
            .NotEmpty().WithMessage("Счёт получателя обязателен для заполнения.");

        RuleFor(x => x.PayloadModel.Amount)
            .GreaterThan(0).WithMessage("Сумма перевода должна быть больше нуля.");

        RuleFor(x => x.PayloadModel.Currency)
            .NotEmpty().WithMessage("Поле 'Валюта' обязательно для заполнения.");

        RuleFor(x => x)
            .Must(x => x.PayloadModel.FromAccountId != x.PayloadModel.ToAccountId)
            .WithMessage("Счёт отправителя и получателя не должны совпадать.");
    }
}