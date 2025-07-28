using AccountService.UserService.Abstractions;
using FluentValidation;

namespace AccountService.Features.Account.GetAccountsByOwnerId;

// ReSharper disable once UnusedMember.Global (Используется в MediatR)
public class GetAccountsByOwnerIdQueryValidator : AbstractValidator<GetAccountsByOwnerIdQuery>
{
    public GetAccountsByOwnerIdQueryValidator(IUserService userService)
    {
        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .WithMessage("ID владельца счёта обязательно для заполнения.")
            .MustAsync((ownerId, _) => userService.IsExistsAsync(ownerId))
            .WithMessage("Владелец не существует");
    }
}