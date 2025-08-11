using FluentValidation;

namespace AccountService.Features.Account.AccrueInterest;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class AccrueInterestCommandValidator : AbstractValidator<AccrueInterestCommand>
{
    public AccrueInterestCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty().WithMessage("AccountId не должен быть пустым");
    }
}