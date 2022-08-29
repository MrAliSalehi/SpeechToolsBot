using System.Collections;
using FFMpegCore;
using Microsoft.CognitiveServices.Speech;
using Serilog.Events;
using SpeechToolsBot.ApiCalls;
using SpeechToolsBot.BackgroundServices;
using SpeechToolsBot.Common.Files;
using SpeechToolsBot.UpdateProcessors;
using Telegram.Bot;

namespace SpeechToolsBot.Common.Extensions;

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

            var configurationRoot = builder
                .SetBasePath(context.HostingEnvironment.ContentRootPath).AddJsonFile($"settings{settingsFileName}.json", false, true)
                .AddEnvironmentVariables()
                .Build();

            var clientId = configurationRoot.GetSection("AzureConfig:ApplicationClientId").Get<string>();
            if (StaticVariables.EnvironmentName == Environments.Development)
            {
                var thumbPrint = configurationRoot.GetSection("AzureConfig:ThumbPrint").Get<string>();
                builder.AddAzureKeyVault("https://qwxpkeyvault.vault.azure.net/", clientId, thumbPrint.GetCertificate());
            }
            else
            {
                var secret = Environment.GetEnvironmentVariable("DOTNET_AzureAuthSecret");
                if (secret is null)
                {
                    foreach (var variable in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine).Cast<DictionaryEntry>().Where(variable => variable.Key.ToString().Contains("DOTNET_AzureAuthSecret")))
                    {
                        secret = variable.Value.ToString();
                        break;
                    }
                }
                builder.AddAzureKeyVault("https://qwxpkeyvault.vault.azure.net/", clientId, secret);
            }
        });
    }

    public static void AddDependencies(this IHostBuilder host)
    {
        host.ConfigureServices((builder, collection) =>
        {
            var botToken = builder.Configuration.GetSection("BotToken").Get<string>();
            collection.AddSingleton<ITelegramBotClient, TelegramBotClient>(_ => new TelegramBotClient(botToken));

            collection.AddSingleton<TextMessage>();
            collection.AddSingleton<AudioMessage>();

            var subscriptionKey = builder.Configuration.GetSection("SpeechApiKey").Get<string>();
            var speechConfig = SpeechConfig.FromSubscription(subscriptionKey, "eastus");

            collection.AddSingleton(_ => new TextToSpeechApi(speechConfig));
            collection.AddSingleton(_ => new SpeechToTextApi(speechConfig));
        });
    }

    public static void AddBackgroundServices(this IHostBuilder host) { host.ConfigureServices(collection => { collection.AddHostedService<UpdateReceiver>(); }); }

    public static void AddDumpCleaner()
    {
        DumpCleaners.CreateImageFolderIfNeeded();
        DumpCleaners.SetDeletionTimeBasedOnEnvironments();
        DumpCleaners.StartTheCleaner();
    }

    public static void AddFfmpeg()
    {
        string workingDir;
        string binFolder;
        if (StaticVariables.EnvironmentName == Environments.Development)
        {
            workingDir = @"D:\Projects\C#\TelegramProducts\BotApi\TextToSpeech\TextToSpeechBot\bin\Debug\net6.0\ffmpeg";
            binFolder = @"D:\Projects\C#\TelegramProducts\BotApi\TextToSpeech\TextToSpeechBot\bin\Debug\net6.0\ffmpeg\bin\";
        }
        else
        {
            workingDir = "/usr/bin";
            binFolder = "/usr/bin/ffmpeg";
        }
        GlobalFFOptions.Configure(options =>
        {
            options.WorkingDirectory = workingDir;
            options.BinaryFolder = binFolder;
        });
    }
}