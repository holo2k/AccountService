using System.Text.Json;
using AccountService.PipelineBehaviors;
using AccountService.UserService.Abstractions;

namespace AccountService.UserService.Implementations;

public class UserService : IUserService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly List<User> _users = new()
    {
        new User
        {
            Id = Guid.Parse("1d22cb6b-4d05-4c80-aa9d-8a4e5eb37656"),
            Email = "sample_email@yandex.ru",
            FullName = "Иванов Никита Васильевич",
            Password = "password",
            Salt = Guid.NewGuid().ToString()
        },
        new User
        {
            Id = Guid.Parse("43007588-4211-492f-ace0-f5b10aefe26b"),
            Email = "sample_email@yandex.ru",
            FullName = "Павлов Никита Васильевич",
            Password = "password",
            Salt = Guid.NewGuid().ToString()
        },
        new User
        {
            Id = Guid.Parse("4650ec28-5afc-4bb2-8f47-90550012646e"),
            Email = "sample_email@yandex.ru",
            FullName = "Николаев Никита Васильевич",
            Password = "password",
            Salt = Guid.NewGuid().ToString()
        }
    };

    public UserService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public Task<bool> IsExistsAsync(Guid ownerId)
    {
        return Task.FromResult(_users.Any(u => u.Id == ownerId));
    }

    public async Task<MbResult<JsonElement>> GetToken()
    {
        var client = _httpClientFactory.CreateClient();

        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = "account-api",
            ["grant_type"] = "password",
            ["username"] = "testuser",
            ["password"] = "password"
        };

        var authority = _configuration["Keycloak:Authority"];

        if (string.IsNullOrWhiteSpace(authority))
            return MbResult<JsonElement>.Fail(
                new MbError
                {
                    Code = "NotFound",
                    Message =
                        "Keycloak недоступен: сервис запущен изолированно и не имеет доступа к контейнеру аутентификации."
                }
            );

        var tokenUrl = $"{authority}/protocol/openid-connect/token";

        var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(parameters));

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return MbResult<JsonElement>.Fail(
                new MbError
                {
                    Code = "BadRequest",
                    Message = $"Сервер аутентификации вернул отрицательный код состояния. Сообщение: {json}"
                }
            );

        var result = JsonSerializer.Deserialize<JsonElement>(json);

        return MbResult<JsonElement>.Success(result);
    }
}