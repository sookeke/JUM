namespace NotificationService;


using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;

public class Program
{
    public static int Main(string[] args)
    {
        CreateLogger();

        try
        {
            Log.Information("Starting web host");
            var builder = CreateHostBuilder(args);
            builder
                .Build()
                .Run();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
            return 1;
        }
        finally
        {
            // Ensure buffered logs are written to their target sink
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
            .UseSerilog();

    private static void CreateLogger()
    {
        var path = Environment.GetEnvironmentVariable("LogFilePath") ?? "logs";

        try
        {
            if (NotificationServiceConfiguration.IsDevelopment())
            {
                Directory.CreateDirectory(path);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Creating the logging directory failed: {0}", e.ToString());
        }

        var name = Assembly.GetExecutingAssembly().GetName();
        var outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Assembly", $"{name.Name}")
            .Enrich.WithProperty("Version", $"{name.Version}")
            .WriteTo.Console(
                outputTemplate: outputTemplate,
                theme: AnsiConsoleTheme.Code)
            .WriteTo.Async(a => a.File(
                $@"{path}/jum.log",
                outputTemplate: outputTemplate,
                rollingInterval: RollingInterval.Day,
                shared: true))
            .WriteTo.Async(a => a.File(
                new JsonFormatter(),
                $@"{path}/jum.json",
                rollingInterval: RollingInterval.Day))
            .CreateLogger();
    }
}
