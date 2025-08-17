using System.Text.Json.Serialization;
using AccountService.AutoMapper;
using AccountService.CurrencyService.Abstractions;
using AccountService.Features.Account.Consumers;
using AccountService.Features.Outbox.Dispatcher;
using AccountService.Features.Outbox.Service;
using AccountService.Filters;
using AccountService.Infrastructure.Helpers;
using AccountService.Infrastructure.Messaging;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.Infrastructure.Repository.Implementations;
using AccountService.Jobs;
using AccountService.PipelineBehaviors;
using AccountService.Startup.Auth;
using AccountService.Startup.Middleware;
using AccountService.UserService.Abstractions;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Startup;

public static class Startup
{
    private static readonly bool IsTest = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test";

    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();

        services.AddLogger();

        if (IsTest)
        {
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
        }
        else
        {
            var connection = configuration.GetConnectionString("DefaultConnection");
            DbContextInitializer.Initialize(services, connection ?? throw new ArgumentNullException(connection));

            services.AddHangfire(config =>
                config.UsePostgreSqlStorage(options => { options.UseNpgsqlConnection(connection); })
            );
            services.AddHangfireServer();

            services.AddAuthentication(configuration);
            services.AddSwagger(configuration);
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            services.AddHostedService<OutboxDispatcher>();
            services.AddHostedService<AntifraudConsumer>();
            services.AddHostedService<AuditConsumer>();
        }

        services.AddControllers(options => { options.Filters.Add<ModelValidationFilter>(); })
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ISqlExecutor, SqlExecutor>();

        services.AddScoped<IOutboxService, OutboxService>();

        services.AddSingleton<IUserService, UserService.Implementations.UserService>();
        services.AddSingleton<ICurrencyService, CurrencyService.Implementations.CurrencyService>();

        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(Program).Assembly));

        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddEndpointsApiExplorer();

        services.AddCors();
    }


    public static async Task Configure(WebApplication app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();

        app.UseCors("AllowAll");

        app.AddExceptionHandler();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        if (!IsTest)
        {
            if (app.Services.GetRequiredService<IRabbitMqPublisher>() is RabbitMqPublisher publisher)
                await publisher.InitializeAsync();

            await app.MigrateDatabaseAsync();

            var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
            recurringJobManager.AddOrUpdate<InterestAccrualJob>(
                "AccrueInterestJob",
                job => job.RunAsync(),
                Cron.Daily(0, 0));

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new AllowAllDashboardAuthorizationFilter() }
            });

            app.AddSwagger();
        }

        app.MapControllers();
    }
}