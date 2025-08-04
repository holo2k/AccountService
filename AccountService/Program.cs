namespace AccountService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Startup.Startup.ConfigureServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        Startup.Startup.Configure(app);

        app.Run();
    }
}