using AccountService.Features.Account.AddAccount;
using AccountService.Features.Account.CheckAccountOwnership;
using AccountService.Features.Account.CloseDeposit;
using AccountService.Features.Account.DeleteAccount;
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
    /// <response code="401">Не авторизованный запрос</response>
    /// <response code="422">Нарушение правил валидации</response>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(MbResult<ICollection<AccountDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult<ICollection<AccountDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult<ICollection<AccountDto>>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetByUserId(Guid userId)
    {
        var result = await _mediator.Send(new GetAccountsByOwnerIdQuery(userId));
        return this.FromResult(result);
    }

    /// <summary>
    ///     Проверить, что счёт принадлежит указанному владельцу.
    /// </summary>
    /// <param name="ownerId">ID владельца счета.</param>
    /// <param name="accountId">ID счёта.</param>
    /// <response code="200">Счёт принадлежит владельцу</response>
    /// <response code="400">Ошибка во время запроса</response>
    /// <response code="401">Не авторизованный запрос</response>
    /// <response code="404">Счёт не найден</response>
    /// <response code="422">Нарушение правил валидации</response>
    [HttpGet("{accountId}/owner/{ownerId}/exists")]
    [ProducesResponseType(typeof(MbResult<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MbResult<bool>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CheckAccountOwnership(Guid ownerId, Guid accountId)
    {
        var result = await _mediator.Send(new CheckAccountOwnershipQuery(ownerId, accountId));

        return this.FromResult(result);
    }

    /// <summary>
    ///     Получить баланс текущего счёта владельца.
    /// </summary>
    /// <param name="ownerId">ID владельца счета.</param>
    /// <returns>Статус 200 OK, если счёт принадлежит владельцу; 404 Not Found — если нет.</returns>
    /// <response code="200">Баланс успешно получен.</response>
    /// <response code="400">Некорректные параметры запроса.</response>
    /// <response code="401">Не авторизованный запрос</response>
    /// <response code="404">Счёт или владелец не найдены.</response>
    /// <response code="422">Нарушение правил валидации</response>
    [HttpGet("{ownerId}/balance")]
    [ProducesResponseType(typeof(MbResult<decimal>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult<decimal>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult<decimal>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MbResult<decimal>), StatusCodes.Status422UnprocessableEntity)]
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
    /// <response code="401">Не авторизованный запрос</response>
    /// <response code="404">Счёт или владелец не найдены.</response>
    /// <response code="422">Нарушение правил валидации</response>
    [HttpGet("{accountId}/statement")]
    [ProducesResponseType(typeof(MbResult<AccountStatementDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult<AccountStatementDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult<AccountStatementDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MbResult<AccountStatementDto>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetAccountStatement(
        Guid accountId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var query = new GetAccountStatementQuery(accountId, from, to);
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
    /// <response code="201">Счёт успешно создан</response>
    /// <response code="400">Ошибка в команде</response>
    /// <response code="401">Не авторизованный запрос</response>
    /// <response code="422">Нарушение правил валидации</response>
    [HttpPost]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Add([FromBody] AddAccountCommand command)
    {
        var result = await _mediator.Send(command);

        return !result.IsSuccess ? this.FromResult(result) : StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    ///     Закрыть вклад и добавить проценты.
    /// </summary>
    /// <param name="accountId">ID счета-вклада</param>
    /// <returns>Результат операции</returns>
    /// <response code="200">Вклад успешно закрыт и проценты начислены</response>
    /// <response code="400">Ошибка в запросе</response>
    /// <response code="404">Счёт не найден</response>
    /// <response code="409">Конфликт версий</response>
    [HttpPost("{accountId}/close-deposit")]
    [ProducesResponseType(typeof(MbResult<Unit>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult<Unit>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MbResult<Unit>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MbResult<Unit>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CloseDeposit(Guid accountId)
    {
        var result = await _mediator.Send(new CloseDepositCommand(accountId));
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
    /// <response code="400">Ошибка в команде</response>
    /// <response code="401">Не авторизованный запрос</response>
    /// <response code="404">Не найден счёт</response>
    /// <response code="422">Нарушение правил валидации</response>
    [HttpPut("{accountId}")]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status422UnprocessableEntity)]
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
    /// <response code="400">Ошибка во время запроса</response>
    /// <response code="401">Не авторизованный запрос</response>
    /// <response code="404">Не найден счёт</response>
    /// <response code="422">Нарушение правил валидации</response>
    [HttpDelete("{accountId}")]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MbResult<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MbResult<Guid>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Delete(Guid accountId)
    {
        var command = new DeleteAccountCommand(accountId);
        var result = await _mediator.Send(command);
        return this.FromResult(result);
    }
}