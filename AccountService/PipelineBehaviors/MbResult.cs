namespace AccountService.PipelineBehaviors;

public class MbResult<T>
{
    public T? Result { get; set; }
    public MbError? Error { get; set; }

    // ReSharper disable once UnusedMember.Global (Используется в выводе клиенту)
    public bool IsSuccess => Error == null;

    public static MbResult<T> Success(T result)
    {
        return new MbResult<T> { Result = result };
    }

    // ReSharper disable once UnusedMember.Global (Используется в ValidationBehavior)
    public static MbResult<T> Fail(MbError error)
    {
        return new MbResult<T> { Error = error };
    }
}

public class MbError
{
    public string Code { get; set; } = default!;
    public string Message { get; set; } = default!;
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}