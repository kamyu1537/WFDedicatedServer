using WFDS.Server;
using WFDS.Server.Managers;
using WFDS.Server.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3)
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
#if DEBUG
    .MinimumLevel.Debug()
#else
    .MinimumLevel.Information()
#endif
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);
    
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile("appsettings.local.json", optional: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables()
        .Build();
    
    builder.Services.Configure<ServerSetting>(configuration.GetSection("Server"));

    builder.Services.AddSerilog();

    builder.Services.AddSingleton<ActorIdManager>();
    builder.Services.AddSingleton<LobbyManager>();
    builder.Services.AddSingleton<PacketHandler>();
    builder.Services.AddSingleton<ActorManager>();
    builder.Services.AddSingleton<MapManager>();

    builder.Services.AddHostedService<ServerInitializeService>();
    builder.Services.AddHostedService<ConfigurationChangeService>();
    builder.Services.AddHostedService<PacketProcessService>();
    builder.Services.AddHostedService<PacketSendService>();

    builder.Services.AddHostedService<HostSpawnScheduleService>();
    builder.Services.AddHostedService<AmbientSpawnScheduleService>();
    builder.Services.AddHostedService<RequestPingScheduleService>();
    builder.Services.AddHostedService<MetalSpawnScheduleService>();
    builder.Services.AddHostedService<ActorUpdateService>();
    builder.Services.AddHostedService<LobbyUpdateScheduleService>();

    var host = builder.Build();
    
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Logger.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}