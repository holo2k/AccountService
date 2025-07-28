using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace AccountService.Exceptions.Handler;

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

                    case AccountNotFoundException ex:
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            Title = "Счёт не найден",
                            Status = 404,
                            ex.Message
                        });
                        break;

                    case InsufficientFundsException ex:
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            Title = "Недостаточно средств",
                            Status = 400,
                            ex.Message
                        });
                        break;

                    case CurrencyMismatchException ex:
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            Title = "Несовпадение валют",
                            Status = 400,
                            ex.Message
                        });
                        break;

                    case CurrencyNotSupportedException ex:
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            Title = "Валюта не поддерживается",
                            Status = 400,
                            ex.Message
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