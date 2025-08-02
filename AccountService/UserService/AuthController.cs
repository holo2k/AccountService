using Microsoft.AspNetCore.Mvc;

namespace AccountService.UserService;

/// <summary>
///     Контроллер для получения токена от Keycloak.
/// </summary>
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    /// <summary>
    ///     Получить токен для тестового пользователя.
    /// </summary>
    /// <remarks>
    ///     Выполняет запрос к Keycloak с использованием Resource Owner Password Credentials.
    ///     <br />
    ///     Используются следующие учётные данные:
    ///     <ul>
    ///         <li><b>username:</b> testuser</li>
    ///         <li><b>password:</b> password</li>
    ///         <li><b>client_id:</b> account-api</li>
    ///     </ul>
    ///     Возвращает JSON с access_token, refresh_token и прочими параметрами.
    /// </remarks>
    /// <returns>JSON с токеном</returns>
    /// <response code="200">Токен успешно получен</response>
    /// <response code="400">Ошибка при получении токена</response>
    [HttpGet("token")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetToken()
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
            return BadRequest(
                "Keycloak недоступен: сервис запущен изолированно и не имеет доступа к контейнеру аутентификации.");

        var tokenUrl = $"{authority}/protocol/openid-connect/token";

        var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(parameters));

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return BadRequest(json);

        return Content(json, "application/json");
    }
}