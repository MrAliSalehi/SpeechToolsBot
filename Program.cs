try
{
    var host = Host.CreateDefaultBuilder(args).UseSystemd();

    host.AddEnv();

    StartupExtensions.AddDumpCleaner();

    StartupExtensions.AddFfmpeg();

    host.AddSerilog();

    host.AddConfiguration();

    host.AddDependencies();

    host.AddBackgroundServices();

    await host.Build().RunAsync();
}
catch (Exception e)
{
    Log.Error(e, "Application Closed");
}
finally
{
    Log.CloseAndFlush();
}