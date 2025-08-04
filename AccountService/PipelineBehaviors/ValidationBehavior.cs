using FluentValidation;
using MediatR;

namespace AccountService.PipelineBehaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count <= 0) return await next(cancellationToken);

        var first = failures.First();

        var mbError = new MbError
        {
            Code = "ValidationFailure",
            Message = "Ошибка во время валидации",
            ValidationErrors = new Dictionary<string, string[]>
            {
                [first.PropertyName] = new[] { first.ErrorMessage }
            }
        };

        var responseType = typeof(TResponse);
        if (!responseType.IsGenericType || responseType.GetGenericTypeDefinition() != typeof(MbResult<>))
            throw new InvalidOperationException("ValidationBehavior expects TResponse to be MbResult<T>.");

        var failMethod = responseType.GetMethod("Fail", new[] { typeof(MbError) })!;
        var result = failMethod.Invoke(null, new object[] { mbError })!;
        return (TResponse)result;
    }
}