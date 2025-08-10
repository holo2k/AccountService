using System.Text.Json.Serialization;
using AccountService.AutoMapper;
using AccountService.CurrencyService.Abstractions;
using AccountService.Filters;
using AccountService.Infrastructure;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.Infrastructure.Repository.Implementations;
using AccountService.Jobs;
using AccountService.PipelineBehaviors;
using AccountService.UserService.Abstractions;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Startup;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();

        var connection = configuration.GetConnectionString("DefaultConnection");

        DbContextInitializer.Initialize(services, connection ?? throw new ArgumentNullException(connection));

        services.AddControllers(options => { options.Filters.Add<ModelValidationFilter>(); })
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddSingleton<IUserService, UserService.Implementations.UserService>();
        services.AddSingleton<ICurrencyService, CurrencyService.Implementations.CurrencyService>();

        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(Program).Assembly));

        services.AddHangfire(config =>
            config.UsePostgreSqlStorage(options => { options.UseNpgsqlConnection(connection); })
        );

        services.AddHangfireServer();

        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddEndpointsApiExplorer();

        services.AddAuthentication(configuration);
        services.AddSwagger(configuration);
        services.AddCors();
    }


    public static async Task Configure(WebApplication app)
    {
        app.UseCors("AllowAll");

        await app.MigrateDatabaseAsync();

        var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
        recurringJobManager.AddOrUpdate<InterestAccrualJob>(
            "AccrueInterestJob",
            job => job.RunAsync(),
            Cron.Daily(0, 0));

        app.AddSwagger();

        app.AddExceptionHandler();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }
}