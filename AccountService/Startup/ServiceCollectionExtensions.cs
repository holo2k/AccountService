using System.Reflection;
using System.Text.Json;
using AccountService.PipelineBehaviors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

namespace AccountService.Startup;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
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

            c.AddSecurityDefinition("Keycloak OAuth2", new OpenApiSecurityScheme
            {
                Description = "Для входа используйте тестового пользователя - testuser : password",
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(configuration["Keycloak:SwaggerAuthUrl"]!),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID" }
                        }
                    }
                }
            });

            c.AddSecurityDefinition("Keycloak ApiKey", new OpenApiSecurityScheme
            {
                Description = "Для входа введите Bearer [пробел] {access_token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Keycloak OAuth2"
                        }
                    },
                    new[] { "openid" }
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Keycloak ApiKey"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            c.EnableAnnotations();
        });


        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Keycloak:Authority"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidIssuers = new[]
                    {
                        configuration["Keycloak:Authority"],
                        configuration["Keycloak:LocalAuthority"]
                    }
                };
                options.RequireHttpsMetadata = false;

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        var error = new MbError
                        {
                            Code = "Unauthorized",
                            Message = context.ErrorDescription!
                        };

                        var result = MbResult<object>.Fail(error);
                        var json = JsonSerializer.Serialize(result);

                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

                        return context.Response.WriteAsync(json);
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        return services;
    }

    public static void AddLogger(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.File(
                "Logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: true,
                outputTemplate:
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e =>
                    e.Properties.ContainsKey("SourceContext") &&
                    (
                        e.Properties["SourceContext"].ToString().Contains("OutboxDispatcher") ||
                        e.Properties["SourceContext"].ToString().Contains("AuditConsumer") ||
                        // ReSharper disable once StringLiteralTypo (Слова нет в словаре)
                        e.Properties["SourceContext"].ToString().Contains("AntifraudConsumer") ||
                        e.Properties["SourceContext"].ToString().Contains("RabbitMqPublisher")
                    )
                )
                .WriteTo.File("Logs/rabbit-mq-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7,
                    shared: true,
                    outputTemplate:
                    "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"))
            .CreateLogger();
    }
}