namespace AccountService.UserService.Abstractions;

public interface IUserService
{
    Task<bool> IsExistsAsync(Guid ownerId);
}