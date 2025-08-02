using AccountService.Features.Transaction.AddTransaction;
using AccountService.Features.Transaction.TransferBetweenAccounts;
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
    /// <returns>Результат добавления транзакции</returns>
    /// <response code="200">Транзакция успешно добавлена</response>
    /// <response code="400">Ошибка во время проверки или бизнес-логики</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Add([FromBody] AddTransactionCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    ///     Перевести средства между счетами.
    /// </summary>
    /// <param name="command">Команда перевода между счетами</param>
    /// <returns>Результат перевода</returns>
    /// <response code="200">Перевод успешно выполнен</response>
    /// <response code="400">Ошибка во время проверки или бизнес-логики</response>
    [HttpPost("transfer")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Transfer([FromBody] TransferBetweenAccountsCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}