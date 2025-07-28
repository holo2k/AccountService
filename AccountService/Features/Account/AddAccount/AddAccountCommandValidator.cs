using AccountService.CurrencyService.Abstractions;
using AccountService.UserService.Abstractions;
using FluentValidation;

namespace AccountService.Features.Account.AddAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class AddAccountCommandValidator : AbstractValidator<AddAccountCommand>
{
    public AddAccountCommandValidator(IUserService userService, ICurrencyService currencyService)
    {
        RuleFor(x => x.Account.Currency)
            .NotEmpty()
            .WithMessage("Валюта обязательна для заполнения.");

        RuleFor(x => x.Account.Type)
            .IsInEnum()
            .WithMessage("Недопустимый тип счёта.");

        RuleFor(x => x.Account.PercentageRate)
            .NotEmpty()
            .WithMessage("Процентная ставка обязательна для вкладов и кредитов.")
            .GreaterThan(0)
            .WithMessage("Процентная ставка должна быть больше 0.")
            .When(x => x.Account.Type is AccountType.Deposit or AccountType.Credit);

        RuleFor(x => x.Account.Balance)
            .GreaterThan(-1)
            .WithMessage("Баланс должен быть не отрицательным.");

        RuleFor(x => x.Account.PercentageRate)
            .Must(rate => rate == null)
            .WithMessage("Процентная ставка не допускается для расчётного счёта.")
            .When(x => x.Account.Type == AccountType.Checking);

        RuleFor(x => x.Account.OwnerId)
            .MustAsync((ownerId, _) => userService.IsExistsAsync(ownerId))
            .WithMessage("Владелец не существует");

        RuleFor(x => x.Account.Currency)
            .Must(currencyService.IsSupported)
            .WithMessage("Неподдерживаемая валюта");
    }
}