using System.Text.Json.Serialization;
using AccountService.AutoMapper;
using AccountService.CurrencyService.Abstractions;
using AccountService.Filters;
using AccountService.Infrastructure.Repository.Abstractions;
using AccountService.Infrastructure.Repository.Implementations;
using AccountService.PipelineBehaviors;
using AccountService.UserService.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AccountService.Startup;

public static class Startup
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();

        services.AddControllers(options => { options.Filters.Add<ModelValidationFilter>(); })
            .AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

        services.AddSingleton<IAccountRepository, AccountRepository>();
        services.AddSingleton<ITransactionRepository, TransactionRepository>();
        services.AddSingleton<IUserService, UserService.Implementations.UserService>();
        services.AddSingleton<ICurrencyService, CurrencyService.Implementations.CurrencyService>();

        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(Program).Assembly));

        ValidatorOptions.Global.DefaultClassLevelCascadeMode = CascadeMode.Stop;
        ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
        services.AddValidatorsFromAssemblyContaining<Program>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddEndpointsApiExplorer();

        services.AddAuthentication(configuration);
        services.AddSwagger(configuration);
        services.AddCors();
    }


    public static void Configure(WebApplication app)
    {
        app.UseCors("AllowAll");

        app.AddSwagger();

        app.AddExceptionHandler();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
    }
}