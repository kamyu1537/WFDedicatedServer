using Serilog;
using WFDS.Server;
using WFDS.Server.Managers;
using WFDS.Server.Services;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3)
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
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
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile("appsettings.local.json", true, true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
        .AddEnvironmentVariables()
        .Build();

    builder.Services.Configure<ServerSetting>(configuration.GetSection("Server"));

    builder.Services.AddSerilog();

    builder.Services.AddSingleton<ActorIdManager>();
    builder.Services.AddSingleton<LobbyManager>();
    builder.Services.AddSingleton<PacketHandler>();
    builder.Services.AddSingleton<ActorManager>();
    builder.Services.AddSingleton<MapManager>();

    // lobby
    builder.Services.AddHostedService<MainWorker>();
    builder.Services.AddHostedService<ConfigurationChangeService>();
    builder.Services.AddHostedService<LobbyUpdateScheduleService>();
    
    // packet
    builder.Services.AddHostedService<PacketProcessService>();
    builder.Services.AddHostedService<PacketSendService>();
    
    // spawn
    builder.Services.AddHostedService<HostSpawnScheduleService>();
    builder.Services.AddHostedService<AmbientSpawnScheduleService>();
    builder.Services.AddHostedService<RequestPingScheduleService>();
    builder.Services.AddHostedService<MetalSpawnScheduleService>();
    
    // actor
    builder.Services.AddHostedService<ActorUpdateService>();
    builder.Services.AddHostedService<ActorActionService>();

    // server start
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