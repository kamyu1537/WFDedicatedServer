using System.Globalization;
using System.Text.Json;
using Cysharp.Threading;
using Microsoft.EntityFrameworkCore;
using WFDS.Common;
using WFDS.Common.Actor;
using WFDS.Common.Plugin;
using WFDS.Common.Steam;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;
using WFDS.Database;
using WFDS.Server.Core;
using WFDS.Server.Core.Actor;
using WFDS.Server.Core.Chalk;
using WFDS.Server.Core.Configuration;
using WFDS.Server.Core.GameEvent;
using WFDS.Server.Core.Network;
using WFDS.Server.Core.Utils;
using WFDS.Server.Core.Zone;

try
{
    CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
    SystemMonitor.Start();
    LogicLooperPool.InitializeSharedPool(100, Environment.ProcessorCount, RoundRobinLogicLooperPoolBalancer.Instance);

    AppDomain.CurrentDomain.UnhandledException += (_, e) => Log.Logger.LogCritical(e.ExceptionObject as Exception, "unhandled exception");
    AppDomain.CurrentDomain.ProcessExit += (_, _) => Log.Logger.LogWarning("process exit");

    var plugins = PluginManager.LoadPlugins();
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseConsoleLifetime();

    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
        .AddJsonFile("appsettings.local.json", true, true)
        .AddEnvironmentVariables()
        .Build();

    builder.Services.AddExceptionHandler<WebExceptionHandler>();

    var section = configuration.GetSection("Server");
    var setting = section.Get<ServerSetting>();

    ArgumentNullException.ThrowIfNull(setting);

    builder.Services.AddSingleton(Log.Factory);
    builder.Services.AddSingleton(Log.Logger);

    builder.Configuration.Sources.Clear();
    
    builder.Services.Configure<ServerSetting>(section);
    builder.UseMaxCountConfigure(configuration);
    builder.UseDecayTimerConfigure(configuration);

    /////////////////////////////////////////////////////////////////

    builder.Services.AddDbContext<DatabaseContext>(d => { d.UseSqlite("Data Source=./data.db;Cache=Shared"); });

    /////////////////////////////////////////////////////////////////

    builder.Services.AddSingleton(SteamManager.Inst);
    builder.Services.AddSingleton(LobbyManager.Inst);
    builder.Services.AddSingleton(SessionManager.Inst);
    builder.Services.AddSingleton<PacketHandleManager>();
    builder.Services.AddTransient<PlayerLogManager>();
    
    builder.Services.AddSingleton<IActorSettingManager, ActorSettingManager>();
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

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.SupportNonNullableReferenceTypes();
        options.EnableAnnotations();
    });

    builder.Services.Configure<RouteOptions>(x => x.LowercaseUrls = true);
    builder.Services.AddRazorPages();
    builder.Services.AddMvc().AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        x.JsonSerializerOptions.WriteIndented = true;
    });

    // server start
    var app = builder.Build();

    // db
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<DatabaseContext>();
        await context.Database.MigrateAsync();
    }

    app.Services.GetRequiredService<IHostApplicationLifetime>()
        .ApplicationStopped
        .Register(() => LogicLooperPool.Shared.ShutdownAsync(TimeSpan.Zero).Wait());

    app.Services.GetRequiredService<IHostApplicationLifetime>()
        .ApplicationStopped
        .Register(() => LogicLooperPool.Shared.ShutdownAsync(TimeSpan.Zero).Wait());

    app.LoadZones();

    app.UseRouting();
    app.UseStaticFiles();
    app.MapRazorPages();
    app.MapControllers();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.Urls.Add($"http://*:{setting.AdminPort}/");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Logger.LogCritical(ex, "host terminated unexpectedly");
}
finally
{
    await Serilog.Log.CloseAndFlushAsync();
    Environment.Exit(0);
}