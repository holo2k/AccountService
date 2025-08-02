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
    /// <response code="200">Транзакция успешно добавлена</response>
    /// <response code="400">Ошибка валидации или бизнес-логики</response>
    /// <response code="401">Неавторизованный запрос</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Add([FromBody] AddTransactionCommand command)
    {
        var result = await _mediator.Send(command);
        return this.FromResult(result);
    }

    /// <summary>
    ///     Перевести средства между счетами.
    /// </summary>
    /// <param name="command">Команда перевода между счетами</param>
    /// <returns>Идентификатор выполненного перевода</returns>
    /// <response code="200">Перевод успешно выполнен</response>
    /// <response code="400">Ошибка валидации или бизнес-логики</response>
    /// <response code="401">Неавторизованный запрос</response>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Transfer([FromBody] TransferBetweenAccountsCommand command)
    {
        var result = await _mediator.Send(command);
        return this.FromResult(result);
    }
}