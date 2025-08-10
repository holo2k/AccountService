using AccountService.Infrastructure.Repository;
using Microsoft.EntityFrameworkCore;

namespace AccountService.Infrastructure;

public static class DbContextInitializer
{
    public static void Initialize(IServiceCollection services, string conn)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(conn));
    }

    public static async Task Migrate(AppDbContext context)
    {
        await context.Database.MigrateAsync();
        await context.SaveChangesAsync();
    }
}