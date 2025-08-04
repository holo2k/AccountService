using AccountService.PipelineBehaviors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AccountService.Filters;

public class ModelValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid) return;

        var errors = context.ModelState
            .Where(kv => kv.Value?.Errors.Count > 0)
            .ToDictionary(
                kv => kv.Key,
                kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        var result = MbResult<object>.Fail(new MbError
        {
            Code = "ValidationError",
            Message = "Ошибка встроенной валидации",
            ValidationErrors = errors
        });

        context.Result = new JsonResult(result)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}