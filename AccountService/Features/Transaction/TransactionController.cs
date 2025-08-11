using AccountService.Features.Transaction.AddTransaction;
using AccountService.Features.Transaction.TransferBetweenAccounts;
using AccountService.PipelineBehaviors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Features.Transaction;

/// <summary>
///     Контроллер для работы с транзакциями.
/// </summary>
[ApiController]
[Route("transactions")]
[Authorize]
public class TransactionController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///     Добавить транзакцию на счёт.
    /// </summary>
    /// <param name="command">Команда на добавление транзакции</param>
    /// <remarks>
    ///     Возможные значения для <c>Type</c>:
    ///     <br />Credit (Зачисление)
    ///     <br />Debit (Списание)
    /// </remarks>
    /// <returns>Идентификатор созданной транзакции</returns>
    /// <response code="201">Транзакция успешно добавлена</response>
    /// <response code="400">Ошибка в команде</response>
    /// <response code="401">Не авторизованный запрос</response>
    /// <response code="404">Счёт не найден</response>
    /// <response code="409">Недостаточно средств</response>
    /// <response code="422">Ошибка валидации</response>
    [HttpPost]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Add([FromBody] AddTransactionCommand command)
    {
        var result = await _mediator.Send(command);

        return !result.IsSuccess ? this.FromResult(result) : StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    ///     Перевести средства между счетами.
    /// </summary>
    /// <param name="command">Команда перевода между счетами</param>
    /// <returns>Идентификатор выполненного перевода</returns>
    /// <response code="200">Перевод успешно выполнен</response>
    /// <response code="400">Ошибка в команде</response>
    /// <response code="401">Не авторизованный запрос</response>
    /// <response code="404">Один из счетов не найден</response>
    /// <response code="409">Недостаточно средств</response>
    /// <response code="422">Ошибка валидации</response>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Transfer([FromBody] TransferBetweenAccountsCommand command)
    {
        var result = await _mediator.Send(command);
        return this.FromResult(result);
    }
}