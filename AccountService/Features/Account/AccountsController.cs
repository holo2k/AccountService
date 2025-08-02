using AccountService.Features.Account.AddAccount;
using AccountService.Features.Account.DeleteAccount;
using AccountService.Features.Account.GetAccount;
using AccountService.Features.Account.GetAccountBalance;
using AccountService.Features.Account.GetAccountsByOwnerId;
using AccountService.Features.Account.GetAccountStatement;
using AccountService.Features.Account.UpdateAccount;
using AccountService.PipelineBehaviors;
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
    /// <response code="401">Неавторизованный запрос</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(ICollection<AccountDto>), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var query = new GetAccountsByOwnerIdQuery(userId);
        var result = await _mediator.Send(query);

        return this.FromResult(result);
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
    /// <response code="401">Неавторизованный запрос</response>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Add([FromBody] AddAccountCommand command)
    {
        var result = await _mediator.Send(command);
        return this.FromResult(result);
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
    /// <response code="401">Неавторизованный запрос</response>
    [HttpPut("{accountId}")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(Guid accountId, [FromBody] UpdateAccountCommand command)
    {
        command.Account.Id = accountId;
        var result = await _mediator.Send(command);
        return this.FromResult(result);
    }

    /// <summary>
    ///     Удалить счёт по ID.
    /// </summary>
    /// <param name="accountId">Идентификатор счёта</param>
    /// <returns>ID удалённого счёта</returns>
    /// <response code="200">Счёт успешно удалён</response>
    /// <response code="400">Ошибка бизнес-логики</response>
    /// <response code="401">Неавторизованный запрос</response>
    [HttpDelete("{accountId}")]
    [ProducesResponseType(typeof(Guid), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(Guid accountId)
    {
        var command = new DeleteAccountCommand(accountId);
        var result = await _mediator.Send(command);
        return this.FromResult(result);
    }

    /// <summary>
    ///     Проверить, что счёт принадлежит указанному владельцу.
    /// </summary>
    /// <param name="ownerId">ID владельца счета.</param>
    /// <param name="accountId">ID счёта.</param>
    /// <response code="200">Счёт принадлежит владельцу</response>
    /// <response code="400">Счёт не принадлежит владельцу</response>
    /// <response code="401">Неавторизованный запрос</response>
    [HttpGet("{accountId}/owner/{ownerId}/exists")]
    [ProducesResponseType(typeof(AccountDto), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckAccountOwnership(Guid ownerId, Guid accountId)
    {
        var result = await _mediator.Send(new GetAccountQuery(accountId));

        if (!result.IsSuccess || result.Result is null || result.Result.OwnerId != ownerId)
            return BadRequest(result);

        return this.FromResult(result);
    }

    /// <summary>
    ///     Получить баланс текущего счёта владельца.
    /// </summary>
    /// <param name="ownerId">ID владельца счета.</param>
    /// <returns>Статус 200 OK, если счёт принадлежит владельцу; 404 Not Found — если нет.</returns>
    /// <response code="200">Баланс успешно получен.</response>
    /// <response code="400">Некорректные параметры запроса.</response>
    /// <response code="404">Счёт или владелец не найдены.</response>
    /// <response code="401">Неавторизованный запрос</response>
    [HttpGet("{ownerId}/balance")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBalance(Guid ownerId)
    {
        var result = await _mediator.Send(new GetAccountBalanceQuery(ownerId));

        return this.FromResult(result);
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
    /// <response code="401">Неавторизованный запрос</response>
    [HttpGet("{accountId}/statement")]
    [ProducesResponseType(typeof(AccountStatementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAccountStatement(
        Guid accountId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var query = new GetAccountStatementQuery(accountId, from, to);
        var result = await _mediator.Send(query);

        return this.FromResult(result);
    }
}