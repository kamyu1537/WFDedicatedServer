using System.Text.Json;
using Serilog;
using WFDS.Common.ChannelEvents;
using WFDS.Common.Types.Manager;
using WFDS.Plugin;
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
    var plugins = PluginManager.LoadPlugins();
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseConsoleLifetime();

    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile("appsettings.local.json", true, true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
        .AddEnvironmentVariables()
        .Build();

    var section = configuration.GetSection("Server");
    var setting = section.Get<ServerSetting>();
    
    ArgumentNullException.ThrowIfNull(setting, nameof(setting));
    
    builder.Services.Configure<ServerSetting>(section);
    builder.Services.AddSerilog();

    builder.Services.AddSingleton<IMapManager, MapManager>();
    builder.Services.AddSingleton<IPacketHandleManager, PacketHandleManager>();
    builder.Services.AddSingleton<IActorIdManager, ActorIdManager>();
    builder.Services.AddSingleton<IActorManager, ActorManager>();
    builder.Services.AddSingleton<IActorSpawnManager, ActorSpawnManager>();
    builder.Services.AddSingleton<ISessionManager, SessionManager>();

    /////////////////////////////////////////////////////////////////
    // server
    builder.Services.AddHostedService<WFServer>();
    
    // lobby
    builder.Services.AddHostedService<ConfigurationChangeService>();
    builder.Services.AddHostedService<LobbyUpdateScheduleService>();
    
    // packet
    builder.Services.AddPacketHandlers();
    builder.Services.AddHostedService<PacketProcessService>();
    builder.Services.AddHostedService<PacketSendService>();
    
    // spawn
    builder.Services.AddHostedService<HostSpawnScheduleService>();
    builder.Services.AddHostedService<AmbientSpawnScheduleService>();
    builder.Services.AddHostedService<RequestPingScheduleService>();
    builder.Services.AddHostedService<MetalSpawnScheduleService>();
    
    // actor
    builder.Services.AddHostedService<ActorUpdateService>();
    builder.Services.AddHostedService<ActorNetworkShareService>();
    builder.Services.AddChannelEventHandlers();
    builder.Services.AddHostedService<ChannelEventService>();
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
    app.MapControllers();
    
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "WFDS.Server");
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
}