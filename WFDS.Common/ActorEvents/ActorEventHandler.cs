using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WFDS.Common.Types;
using WFDS.Common.Types.Manager;

public interface IActorEvent
{
    public long ActorId { get; }
}

internal interface IInitializer<in T>
{
    bool IsInitialized { get; }
    void Initialize(T t);
}

internal interface IActorEventHandler : IInitializer<IActor>
{
    Type EventType { get; }
    IActor Actor { get; }
    Task HandleAsync(IActorEvent e);
}

public abstract class ActorEventHandler<T> : IActorEventHandler where T : IActorEvent
{
    public Type EventType { get; } = typeof(T);
    public IActor Actor { get; private set; } = null!;
    public bool IsInitialized { get; private set; }

    public void Initialize(IActor actor)
    {
        Actor = actor;
        IsInitialized = true;
    }

    public async Task HandleAsync(IActorEvent e)
    {
        if (!IsInitialized) return;

        if (e is T t)
        {
            await HandleAsync(t);
        }
    }

    protected abstract Task HandleAsync(T e);
}

public static class ServiceCollectionExtensions
{
    public static void AddActorEventHandlers(this IServiceCollection services)
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var actorEventHandlerType = typeof(IActorEventHandler);

        var actorEventHandlerTypes = allAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Distinct()
            .Where(type => actorEventHandlerType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface);

        services.TryAddEnumerable(actorEventHandlerTypes.Select(CreateServiceDescriptor));
    }

    private static ServiceDescriptor CreateServiceDescriptor(Type type)
    {
        return new ServiceDescriptor(typeof(IActorEventHandler), type, ServiceLifetime.Transient);
    }
}

public static class ActorEventChannel
{
    internal static readonly Channel<IActorEvent> Channel = System.Threading.Channels.Channel.CreateBounded<IActorEvent>(new BoundedChannelOptions(1000)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = false
    });
    internal static readonly SemaphoreSlim Semaphore = new(0);

    public static async Task PublishAsync(IActorEvent e)
    {
        try
        {
            Semaphore.Release();
            await Channel.Writer.WriteAsync(e);
        }
        catch (ChannelClosedException ex)
        {
            Console.WriteLine($"channel closed: {ex}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    public static async Task WaitAsync()
    {
        Semaphore.Release();
        await Semaphore.WaitAsync();
    }
}

public class ActorEventService(IServiceProvider provider, ILogger<ActorEventService> logger, IActorManager actorManager) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var e in ActorEventChannel.Channel.Reader.ReadAllAsync(stoppingToken))
        {
            await HandleEventAsync(e);
            await ActorEventChannel.WaitAsync();
        }

        ActorEventChannel.Channel.Writer.Complete();
        logger.LogInformation("actor event service stopped");
    }

    private async Task HandleEventAsync(IActorEvent e)
    {
        try
        {
            await using var scope = provider.CreateAsyncScope();
            var handlers = provider.GetServices<IActorEventHandler>();
            await Task.WhenAll(handlers
                .Where(x => x.EventType == e.GetType())
                .Select(x => (actorManager.GetActor(e.ActorId), x))
                .Where(x => x.Item1 != null)
                .Select(x =>
                {
                    x.Item2.Initialize(x.Item1!);
                    return x.Item2.HandleAsync(e);
                }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "failed to handle event");
        }
    }
}