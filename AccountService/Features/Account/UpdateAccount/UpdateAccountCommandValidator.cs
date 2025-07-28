using FluentValidation;

namespace AccountService.Features.Account.UpdateAccount;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class UpdateAccountCommandValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountCommandValidator()
    {
        RuleFor(x => x.Account.Id)
            .NotEmpty()
            .WithMessage("Идентификатор счёта обязателен.");

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

        RuleFor(x => x.Account.PercentageRate)
            .Must(rate => rate == null)
            .WithMessage("Процентная ставка не допускается для расчётного счёта.")
            .When(x => x.Account.Type == AccountType.Checking);
    }
}