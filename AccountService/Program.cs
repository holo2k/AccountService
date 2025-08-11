namespace AccountService;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Startup.Startup.ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        await Startup.Startup.Configure(app);

        await app.RunAsync();
    }
}