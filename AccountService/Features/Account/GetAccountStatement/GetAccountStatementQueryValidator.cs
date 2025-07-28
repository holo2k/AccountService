using FluentValidation;

namespace AccountService.Features.Account.GetAccountStatement;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class GetAccountStatementQueryValidator : AbstractValidator<GetAccountStatementQuery>
{
    public GetAccountStatementQueryValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().WithMessage("Идентификатор счёта обязателен.");
        RuleFor(x => x.From)
            .NotEmpty()
            .WithMessage("Дата начала периода обязательна.");
        RuleFor(x => x.To)
            .NotEmpty()
            .WithMessage("Дата окончания периода обязательна.");
        RuleFor(x => x)
            .Must(x => x.From <= x.To)
            .WithMessage("Дата начала периода не может быть позже даты окончания.");
    }
}