using FSDS.Server.Managers;
using FSDS.Server.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .MinimumLevel.Debug()
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog();

    builder.Services.AddSingleton<ActorIdManager>();
    builder.Services.AddSingleton<LobbyManager>();
    builder.Services.AddSingleton<PacketHandler>();
    builder.Services.AddSingleton<ActorManager>();
    builder.Services.AddSingleton<MapManager>();
    
    builder.Services.AddHostedService<ServerInitializeService>();
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
