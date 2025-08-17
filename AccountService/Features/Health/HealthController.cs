using AccountService.Features.Health.HealthCheck;
using AccountService.PipelineBehaviors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Features.Health;

/// <summary>
///     Контроллер для проверки состояния сервиса (health checks)
/// </summary>
[ApiController]
[Route("health")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    private readonly IMediator _mediator;

    public HealthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///     Live endpoint — просто проверка, что сервис работает
    /// </summary>
    /// <returns>Статус сервиса</returns>
    /// <response code="200">Сервис жив и доступен</response>
    [HttpGet("live")]
    [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status200OK)]
    public IActionResult Live()
    {
        var result = MbResult<object>.Success(new { status = "UP" });
        return this.FromResult(result);
    }

    /// <summary>
    ///     Ready endpoint — проверка готовности к работе (готовность к обработке событий и работе с БД)
    /// </summary>
    /// <returns>Информация о готовности сервиса</returns>
    /// <response code="200">Сервис готов</response>
    /// <response code="503">Сервис не готов (RabbitMQ или база недоступны)</response>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(MbResult<HealthDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult<HealthDto>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Ready()
    {
        var result = await _mediator.Send(new HealthCheckQuery());
        return this.FromResult(result);
    }
}