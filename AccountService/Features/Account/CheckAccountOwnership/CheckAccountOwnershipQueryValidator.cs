using FluentValidation;

namespace AccountService.Features.Account.CheckAccountOwnership;

public class CheckAccountOwnershipQueryValidator : AbstractValidator<CheckAccountOwnershipQuery>
{
    public CheckAccountOwnershipQueryValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty().WithMessage("OwnerId обязателен");
        RuleFor(x => x.AccountId).NotEmpty().WithMessage("AccountId обязателен");
    }
}