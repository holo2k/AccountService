using AccountService.UserService.Abstractions;

namespace AccountService.UserService.Implementations;

public class UserService : IUserService
{
    private readonly List<User> _users = new()
    {
        new User
        {
            Id = Guid.Parse("1d22cb6b-4d05-4c80-aa9d-8a4e5eb37656"),
            Email = "sample_email@yandex.ru",
            FullName = "Беликов Никита Васильевич",
            Password = "password",
            Salt = Guid.NewGuid().ToString()
        },
        new User
        {
            Id = Guid.Parse("43007588-4211-492f-ace0-f5b10aefe26b"),
            Email = "sample_email@yandex.ru",
            FullName = "Беликов Никита Васильевич",
            Password = "password",
            Salt = Guid.NewGuid().ToString()
        },
        new User
        {
            Id = Guid.Parse("4650ec28-5afc-4bb2-8f47-90550012646e"),
            Email = "sample_email@yandex.ru",
            FullName = "Беликов Никита Васильевич",
            Password = "password",
            Salt = Guid.NewGuid().ToString()
        }
    };

    public Task<bool> IsExistsAsync(Guid ownerId)
    {
        return Task.FromResult(_users.Any(u => u.Id == ownerId));
    }
}