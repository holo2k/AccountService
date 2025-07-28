using FluentValidation;

namespace AccountService.Features.Transaction.AddTransaction;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class AddTransactionCommandValidator : AbstractValidator<AddTransactionCommand>
{
    public AddTransactionCommandValidator()
    {
        RuleFor(x => x.Transaction.Amount)
            .GreaterThan(0)
            .WithMessage("Сумма транзакции должна быть больше 0");

        RuleFor(x => x.Transaction.Currency)
            .NotEmpty()
            .WithMessage("Валюта транзакции должна быть указана");

        RuleFor(x => x.Transaction.Type)
            .IsInEnum()
            .WithMessage("Тип транзакции указан неверно");

        RuleFor(x => x.Transaction.AccountId)
            .NotEmpty()
            .WithMessage("Идентификатор аккаунта обязателен");

        RuleFor(x => x.Transaction.Description)
            .MaximumLength(500)
            .WithMessage("Описание транзакции не должно превышать 500 символов")
            .When(x => !string.IsNullOrEmpty(x.Transaction.Description));
    }
}