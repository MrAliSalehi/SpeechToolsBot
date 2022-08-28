using Serilog;
using Serilog.Events;
using Telegram.Bot;
using WorkerService1.BackgroundServices;
using WorkerService1.Common.Files;
using WorkerService1.UpdateProcessors;

namespace WorkerService1.Common.Extensions;

internal static class StartupExtensions
{
    public static void AddEnv(this IHostBuilder host)
    {
#if DEBUG
        host.UseEnvironment(Environments.Development);
        StaticVariables.EnvironmentName = Environments.Development;
#else
        StaticVariables.EnvironmentName = Environments.Production;
        host.UseEnvironment(Environments.Production);

#endif
    }

    public static void AddSerilog(this IHostBuilder host)
    {
        var normalLogs = FileCreator.CreateFile().Folder("Logs").FileNameAndFormat("application", "logs").BuildPathAndFile();

        var configuration = new LoggerConfiguration().MinimumLevel.Debug().MinimumLevel.Override("Microsoft", LogEventLevel.Information).Enrich.FromLogContext().WriteTo.Async(configuration => configuration.File(normalLogs, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true));

        if (StaticVariables.EnvironmentName == Environments.Development)
            configuration.WriteTo.Console();

        Log.Logger = configuration.CreateLogger();

        host.ConfigureLogging(builder => { builder.AddSerilog(Log.Logger); });
    }

    public static void AddConfiguration(this IHostBuilder host)
    {
        host.ConfigureAppConfiguration((context, builder) =>
        {
            var settingsFileName = StaticVariables.EnvironmentName == Environments.Development ? "" : ".prod";

            builder.SetBasePath(context.HostingEnvironment.ContentRootPath).AddJsonFile($"settings{settingsFileName}.json", false, true).Build();
        });
    }

    public static void AddDependencies(this IHostBuilder host)
    {
        host.ConfigureServices((builder, collection) =>
        {
            var botToken = builder.Configuration.GetSection("BotToken").Get<string>();
            collection.AddSingleton<ITelegramBotClient, TelegramBotClient>(_ => new TelegramBotClient(botToken));
            collection.AddSingleton<TextMessage>();
        });
    }

    public static void AddBackgroundServices(this IHostBuilder host)
    {
        host.ConfigureServices(collection => { collection.AddHostedService<UpdateReceiver>(); });
    }
}