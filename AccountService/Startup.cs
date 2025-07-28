using System.Reflection;
using System.Text.Json.Serialization;
using AccountService.AutoMapper;
using AccountService.CurrencyService.Abstractions;
using AccountService.Exceptions.Handler;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.Infrastructure.Repository.Implementations;
using AccountService.PipelineBehaviors;
using AccountService.UserService.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.OpenApi.Models;

namespace AccountService;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddSingleton<IAccountRepository, AccountRepository>();
        services.AddSingleton<ITransactionRepository, TransactionRepository>();
        services.AddSingleton<IUserService, UserService.Implementations.UserService>();
        services.AddSingleton<ICurrencyService, CurrencyService.Implementations.CurrencyService>();

        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        services.AddMediatR(c =>
            c.RegisterServicesFromAssembly(typeof(Program).Assembly));
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Account Service API",
                Version = "v1",
                Description = "API для управления банковскими счетами и транзакциями"
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
        });
    }

    public static void Configure(WebApplication app)
    {
        app.AddSwagger();

        app.AddExceptionHandler();

        app.UseRouting();
        app.MapControllers();
    }

    public static void AddSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Account Service API");
            options.RoutePrefix = string.Empty;
        });
    }
}