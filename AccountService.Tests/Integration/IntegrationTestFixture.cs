using AccountService.Infrastructure.Repository;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AccountService.Tests.Integration;

public class IntegrationTestFixture : IAsyncLifetime
{
    public PostgreSqlContainer PgContainer { get; private set; } = null!;
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        PgContainer = new PostgreSqlBuilder()
            .WithDatabase("test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await PgContainer.StartAsync();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");

                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(PgContainer.GetConnectionString()));
                });
            });


        Client = Factory.CreateClient();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await PgContainer.StopAsync();
    }
}