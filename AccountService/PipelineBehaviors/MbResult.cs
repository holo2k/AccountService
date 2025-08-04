using Microsoft.AspNetCore.Mvc;

namespace AccountService.PipelineBehaviors;

/// <summary>
///     Обёртка для результата выполнения запроса.
/// </summary>
/// <typeparam name="T">Тип возвращаемого результата.</typeparam>
public class MbResult<T>
{
    /// <summary>
    ///     Успешный результат выполнения запроса.
    /// </summary>
    public T? Result { get; set; }

    /// <summary>
    ///     Информация об ошибке, если запрос завершился неуспешно.
    /// </summary>
    public MbError? Error { get; set; }

    /// <summary>
    ///     Признак успешности выполнения запроса.
    /// </summary>
    public bool IsSuccess => Error == null;

    /// <summary>
    ///     Создаёт успешный результат.
    /// </summary>
    public static MbResult<T> Success(T result)
    {
        return new MbResult<T> { Result = result };
    }

    /// <summary>
    ///     Создаёт результат с ошибкой.
    /// </summary>
    public static MbResult<T> Fail(MbError error)
    {
        return new MbResult<T> { Error = error };
    }
}

/// <summary>
///     Представляет ошибку, возникшую при обработке запроса.
/// </summary>
public class MbError
{
    /// <summary>
    ///     Код ошибки.
    /// </summary>
    public string Code { get; set; } = default!;

    /// <summary>
    ///     Описание ошибки.
    /// </summary>
    public string Message { get; set; } = default!;

    /// <summary>
    ///     Информация о валидационных ошибках.
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}

public static class MbResultExtensions
{
    public static IActionResult FromResult<T>(this ControllerBase controller, MbResult<T> result)
    {
        if (result.Result is null)
            return controller.NotFound(result);

        return result.IsSuccess
            ? controller.Ok(result)
            : controller.BadRequest(result);
    }
}