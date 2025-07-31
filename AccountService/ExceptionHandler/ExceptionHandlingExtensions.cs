using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace AccountService.ExceptionHandler;

public static class ExceptionHandlingExtensions
{
    public static void AddExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionFeature?.Error;

                context.Response.ContentType = "application/json";

                switch (exception)
                {
                    case ValidationException validationException:
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            Title = "Ошибка во время проверки",
                            Status = 400,
                            Errors = validationException.Errors.Select(e => new
                            {
                                e.PropertyName,
                                e.ErrorMessage
                            })
                        });
                        break;

                    default:
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            Title = "Внутренняя ошибка сервера",
                            Status = 500,
                            exception?.Message
                        });
                        break;
                }
            });
        });
    }
}