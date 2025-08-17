using AccountService.Features.Account.Consumers;
using AccountService.Features.Outbox.Dispatcher;
using AccountService.Infrastructure.Messaging;
using AccountService.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace AccountService.Tests.Integration.Common;

public class IntegrationTestFixture : IAsyncLifetime
{
    public PostgreSqlContainer PgContainer { get; private set; } = null!;
    public RabbitMqContainer RabbitContainer { get; private set; } = null!;
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        PgContainer = new PostgreSqlBuilder()
            .WithDatabase("test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        RabbitContainer = new RabbitMqBuilder()
            // ReSharper disable once StringLiteralTypo (В словаре нет слова)
            .WithImage("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithExposedPort(5672)
            .Build();

        await Task.WhenAll(PgContainer.StartAsync(), RabbitContainer.StartAsync());

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                var rabbitHost = RabbitContainer.Hostname;
                var rabbitPort = RabbitContainer.GetMappedPublicPort(5672);

                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
                // ReSharper disable once StringLiteralTypo (В словаре нет слова)
                Environment.SetEnvironmentVariable("RABBITMQ_HOST", rabbitHost);
                // ReSharper disable once StringLiteralTypo (В словаре нет слова)
                Environment.SetEnvironmentVariable("RABBITMQ_PORT", rabbitPort.ToString());

                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(PgContainer.GetConnectionString()));

                    services.AddSingleton<IRabbitMqPublisher>(sp =>
                        new RabbitMqPublisher(
                            sp.GetRequiredService<ILogger<RabbitMqPublisher>>(),
                            rabbitHost,
                            rabbitPort
                        ));

                    services.AddHostedService<OutboxDispatcher>();
                    services.AddHostedService<AntifraudConsumer>();

                    services.AddSingleton<RabbitMqInitializer>();
                });
            });

        Client = Factory.CreateClient();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        var rabbitInitializer = Factory.Services.GetRequiredService<RabbitMqInitializer>();
        await rabbitInitializer.StartAsync(CancellationToken.None);
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            PgContainer.StopAsync(),
            RabbitContainer.StopAsync()
        );
    }

    public async Task StopRabbitMqAsync()
    {
        // ReSharper disable once StringLiteralTypo (В словаре нет слова)
        await RabbitContainer.ExecAsync(new List<string> { "rabbitmqctl", "stop_app" });
    }

    public async Task StartRabbitMqAsync()
    {
        // ReSharper disable once StringLiteralTypo (В словаре нет слова)
        await RabbitContainer.ExecAsync(new List<string> { "rabbitmqctl", "start_app" });
    }
}

public class RabbitMqInitializer : IHostedService
{
    private readonly IRabbitMqPublisher _rabbitMqPublisher;

    public RabbitMqInitializer(IRabbitMqPublisher rabbitMqPublisher)
    {
        _rabbitMqPublisher = rabbitMqPublisher;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _rabbitMqPublisher.InitializeAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}