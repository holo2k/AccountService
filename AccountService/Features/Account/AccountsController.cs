using AccountService.Features.Account.AddAccount;
using AccountService.Features.Account.DeleteAccount;
using AccountService.Features.Account.GetAccount;
using AccountService.Features.Account.GetAccountBalance;
using AccountService.Features.Account.GetAccountsByOwnerId;
using AccountService.Features.Account.GetAccountStatement;
using AccountService.Features.Account.UpdateAccount;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Features.Account;

/// <summary>
///     Контроллер для работы с банковскими счетами.
/// </summary>
[ApiController]
[Route("accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    ///     Получить счета пользователя по его ID.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns>Список счетов пользователя</returns>
    /// <response code="200">Счета успешно получены</response>
    /// <response code="400">Некорректный запрос</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(IEnumerable<AccountDto>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var query = new GetAccountsByOwnerIdQuery(userId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    ///     Создать новый счёт.
    /// </summary>
    /// <remarks>
    ///     Возможные значения для <c>Type</c>:
    ///     <br />Checking (Текущий счёт)
    ///     <br />Deposit (Депозитный счёт)
    ///     <br />Credit (Кредитный счёт)
    /// </remarks>
    /// <param name="command">Данные нового счёта</param>
    /// <returns>ID созданного счёта</returns>
    /// <response code="200">Счёт успешно создан</response>
    /// <response code="400">Ошибка во время проверки или бизнес-логики</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Add([FromBody] AddAccountCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    ///     Обновить счёт по ID.
    /// </summary>
    /// <remarks>
    ///     Возможные значения для <c>Type</c>:
    ///     <br />Checking (Текущий счёт)
    ///     <br />Deposit (Депозитный счёт)
    ///     <br />Credit (Кредитный счёт)
    /// </remarks>
    /// <param name="accountId">Идентификатор счёта</param>
    /// <param name="command">Данные для обновления</param>
    /// <returns>ID обновленного счёта</returns>
    /// <response code="200">Счёт успешно обновлён</response>
    /// <response code="400">Ошибка во время проверки или бизнес-логики</response>
    [HttpPut("{accountId}")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Update(Guid accountId, [FromBody] UpdateAccountCommand command)
    {
        command.Account.Id = accountId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    ///     Удалить счёт по ID.
    /// </summary>
    /// <param name="accountId">Идентификатор счёта</param>
    /// <returns>ID удалённого счёта</returns>
    /// <response code="200">Счёт успешно удалён</response>
    /// <response code="400">Ошибка бизнес-логики</response>
    [HttpDelete("{accountId}")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Delete(Guid accountId)
    {
        var command = new DeleteAccountCommand(accountId);
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    ///     Проверить, что счёт принадлежит указанному владельцу.
    /// </summary>
    /// <param name="ownerId">ID владельца счета.</param>
    /// <param name="accountId">ID счёта.</param>
    /// <returns>Статус 200 OK, если счёт принадлежит владельцу; 404 Not Found — если нет.</returns>
    [HttpGet("{accountId}/owner/{ownerId}/exists")]
    public async Task<IActionResult> CheckAccountOwnership(Guid ownerId, Guid accountId)
    {
        var account = await _mediator.Send(new GetAccountQuery(accountId));
        if (account.Result!.OwnerId != ownerId)
            return NotFound("Счёт не найден у данного владельца.");

        return Ok(true);
    }

    /// <summary>
    ///     Получить баланс текущего счёта владельца.
    /// </summary>
    /// <param name="ownerId">ID владельца счета.</param>
    /// <returns>Статус 200 OK, если счёт принадлежит владельцу; 404 Not Found — если нет.</returns>
    /// <response code="200">Баланс успешно получен.</response>
    /// <response code="400">Некорректные параметры запроса.</response>
    /// <response code="404">Счёт или владелец не найдены.</response>
    [HttpGet("{ownerId}/balance")]
    public async Task<IActionResult> GetBalance(Guid ownerId)
    {
        var balance = await _mediator.Send(new GetAccountBalanceQuery(ownerId));

        return Ok(balance);
    }

    /// <summary>
    ///     Получить выписку по счёту клиента за указанный период.
    /// </summary>
    /// <param name="accountId">Идентификатор счёта.</param>
    /// <param name="from">Дата начала периода (включительно).</param>
    /// <param name="to">Дата окончания периода (включительно).</param>
    /// <returns>Список транзакций и информация о счёте за указанный период.</returns>
    /// <response code="200">Выписка успешно получена.</response>
    /// <response code="400">Некорректные параметры запроса.</response>
    /// <response code="404">Счёт или владелец не найдены.</response>
    [HttpGet("{accountId}/statement")]
    public async Task<IActionResult> GetAccountStatement(
        Guid accountId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var query = new GetAccountStatementQuery(accountId, from, to);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}