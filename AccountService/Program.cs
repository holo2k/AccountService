using Serilog;

namespace AccountService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Startup.Startup.ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

        builder.Host.UseSerilog();

        var app = builder.Build();

        await Startup.Startup.Configure(app);

        await app.RunAsync();
    }
}