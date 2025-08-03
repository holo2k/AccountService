using AccountService.PipelineBehaviors;
using AccountService.UserService.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.UserService;

/// <summary>
///     Контроллер для получения токена от Keycloak.
/// </summary>
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
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
    /// <response code="404">Сервер авторизации не найден</response>
    [HttpGet("token")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetToken()
    {
        var token = await _userService.GetToken();

        return !token.IsSuccess ? this.FromResult(token) : Ok(token);
    }
}