using System.Text.Json;
using Cysharp.Threading;
using Serilog;
using WFDS.Common.Actor;
using WFDS.Common.Plugin;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Server.Core;
using WFDS.Server.Core.Actor;
using WFDS.Server.Core.Chalk;
using WFDS.Server.Core.Configuration;
using WFDS.Server.Core.GameEvent;
using WFDS.Server.Core.Network;
using WFDS.Server.Core.Utils;
using WFDS.Server.Core.Zone;

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
    SystemMonitor.Start();
    LogicLooperPool.InitializeSharedPool(100, Environment.ProcessorCount, RoundRobinLogicLooperPoolBalancer.Instance);

    AppDomain.CurrentDomain.UnhandledException += (_, e) => Log.Logger.Fatal(e.ExceptionObject as Exception, "unhandled exception");
    AppDomain.CurrentDomain.ProcessExit += (_, _) => Log.Logger.Information("process exit");

    var plugins = PluginManager.LoadPlugins();
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseConsoleLifetime();

    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile("appsettings.local.json", true, true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
        .AddEnvironmentVariables()
        .Build();

    builder.Services.AddExceptionHandler<WebExceptionHandler>();

    var section = configuration.GetSection("Server");
    var setting = section.Get<ServerSetting>();
    ArgumentNullException.ThrowIfNull(setting, nameof(setting));

    builder.Services.AddSerilog();

    builder.Services.Configure<ServerSetting>(section);

    builder.Services.AddSingleton(SteamManager.Inst);
    builder.Services.AddSingleton(LobbyManager.Inst);
    builder.Services.AddSingleton(SessionManager.Inst);
    builder.Services.AddSingleton<PacketHandleManager>();

    builder.Services.AddSingleton<IZoneManager, ZoneManager>();
    builder.Services.AddSingleton<IActorIdManager, ActorIdManager>();
    builder.Services.AddSingleton<IActorManager, ActorManager>();
    builder.Services.AddSingleton<IActorSpawnManager, ActorSpawnManager>();

    builder.Services.AddSingleton<ICanvasManager, CanvasManager>();

    /////////////////////////////////////////////////////////////////

    builder.Services.AddPacketHandlers();
    builder.Services.AddGameEventHandlers();

    /////////////////////////////////////////////////////////////////

    builder.Services.AddHostedService<WebFishingServer>();
    builder.Services.AddHostedService<ConfigurationChangeService>();

    builder.Services.AddHostedService<PacketProcessService>();
    builder.Services.AddHostedService<PacketSendService>();
    builder.Services.AddHostedService<ActorTickService>();
    builder.Services.AddHostedService<ActorNetworkShareService>();
    builder.Services.AddHostedService<GameEventService>();

    builder.Services.AddHostedService<LobbyUpdateScheduleService>();
    builder.Services.AddHostedService<RequestPingScheduleService>();
    builder.Services.AddHostedService<ActorSetZoneScheduleService>();

    builder.Services.AddHostedService<HostSpawnScheduleService>();
    builder.Services.AddHostedService<AmbientSpawnScheduleService>();
    builder.Services.AddHostedService<MetalSpawnScheduleService>();

    /////////////////////////////////////////////////////////////////
    // plugin
    foreach (var plugin in plugins)
    {
        plugin.EventHandlers.ToList().ForEach(x => builder.Services.AddTransient(x));
        plugin.PacketHandlers.ToList().ForEach(x => builder.Services.AddTransient(x));
    }

    builder.Services.AddSerilog(Log.Logger);

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SupportNonNullableReferenceTypes();
        options.EnableAnnotations();
    });

    builder.Services.Configure<RouteOptions>(x => x.LowercaseUrls = true);
    builder.Services.AddMvc().AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        x.JsonSerializerOptions.WriteIndented = true;
    });

    // server start
    var app = builder.Build();
    app.Services.GetRequiredService<IHostApplicationLifetime>()
        .ApplicationStopped
        .Register(() => LogicLooperPool.Shared.ShutdownAsync(TimeSpan.Zero).Wait());
    
    app.Services.GetRequiredService<IHostApplicationLifetime>()
        .ApplicationStopped
        .Register(() => LogicLooperPool.Shared.ShutdownAsync(TimeSpan.Zero).Wait());
    
    app.LoadZones();

    app.UseRouting();
    app.UseStaticFiles();
    app.MapControllers();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "WEBFISHING Dedicated Server");
        options.RoutePrefix = "";
    });

    app.Urls.Add($"http://*:{setting.AdminPort}/");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Logger.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
    Environment.Exit(0);
}