using System.Text.Json;
using AccountService.PipelineBehaviors;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace AccountService.Startup;

public static class WebAppExtensions
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

                MbResult<object> result;
                int statusCode;

                switch (exception)
                {
                    case ValidationException validationException:
                        statusCode = StatusCodes.Status400BadRequest;
                        var firstError = validationException.Errors.First();
                        result = MbResult<object>.Fail(new MbError
                        {
                            Code = "ValidationError",
                            Message = $"{firstError.ErrorMessage}",
                            ValidationErrors = new Dictionary<string, string[]>
                            {
                                [firstError.PropertyName] = new[] { firstError.ErrorMessage }
                            }
                        });
                        break;

                    default:
                        statusCode = StatusCodes.Status500InternalServerError;
                        result = MbResult<object>.Fail(new MbError
                        {
                            Code = "InternalServerError",
                            Message = $"{exception!.Message}"
                        });
                        break;
                }

                context.Response.StatusCode = statusCode;
                var json = JsonSerializer.Serialize(result);
                await context.Response.WriteAsync(json);
            });
        });
    }

    public static void AddSwagger(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/")
            {
                context.Response.Redirect("/swagger", true);
                return;
            }

            await next();
        });

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Account Service API");
            c.RoutePrefix = "swagger";

            c.OAuthClientId("account-api");

            c.OAuthScopes("openid", "profile", "email", "roles");
        });
    }
}