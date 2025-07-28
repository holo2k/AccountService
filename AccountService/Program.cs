namespace AccountService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Startup.ConfigureServices(builder.Services);

        var app = builder.Build();

        Startup.Configure(app);

        app.Run();
    }
}