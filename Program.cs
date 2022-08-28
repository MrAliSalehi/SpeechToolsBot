try
{
    var host = Host.CreateDefaultBuilder(args);

    host.AddEnv();

    StartupExtensions.AddDumpCleaner();

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