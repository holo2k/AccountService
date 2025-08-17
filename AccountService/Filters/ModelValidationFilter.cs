using AccountService.PipelineBehaviors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AccountService.Filters;

public class ModelValidationFilter : IActionFilter
{
    private readonly ILogger<ModelValidationFilter> _logger;

    public ModelValidationFilter(ILogger<ModelValidationFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;

        var errors = context.ModelState
            .Where(kv => kv.Value?.Errors.Count > 0)
            .ToDictionary(
                kv => kv.Key,
                kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        var mbError = new MbError
        {
            Code = "ValidationError",
            Message = "Ошибка встроенной валидации",
            ValidationErrors = errors
        };

        var result = MbResult<object>.Fail(mbError);

        _logger.LogError("Model validation failed: {ErrorCode}, Message: {ErrorMessage}, Errors: {@ValidationErrors}",
            mbError.Code,
            mbError.Message,
            mbError.ValidationErrors);

        context.Result = new JsonResult(result)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}