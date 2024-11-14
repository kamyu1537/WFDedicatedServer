using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WFDS.Common.ChannelEvents;

public interface IChannelEvent
{
}

public interface IChannelEventHandler
{
    Type EventType { get; }
    Task HandleAsync(IChannelEvent e);
}

public abstract class ChannelEventHandler<T> : IChannelEventHandler where T : IChannelEvent
{
    public Type EventType { get; } = typeof(T);

    public async Task HandleAsync(IChannelEvent e)
    {
        if (e is T t)
        {
            await HandleAsync(t);
        }
    }

    protected abstract Task HandleAsync(T e);
}

public static class ServiceCollectionExtensions
{
    public static void AddChannelEventHandlers(this IServiceCollection services)
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var actorEventHandlerType = typeof(IChannelEventHandler);

        var actorEventHandlerTypes = allAssemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Distinct()
            .Where(type => actorEventHandlerType.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface);

        services.TryAddEnumerable(actorEventHandlerTypes.Select(CreateServiceDescriptor));
    }

    private static ServiceDescriptor CreateServiceDescriptor(Type type)
    {
        return new ServiceDescriptor(typeof(IChannelEventHandler), type, ServiceLifetime.Transient);
    }
}

public static class ChannelEvent
{
    internal static readonly Channel<IChannelEvent> Channel = System.Threading.Channels.Channel.CreateBounded<IChannelEvent>(new BoundedChannelOptions(1000)
    {
        FullMode = BoundedChannelFullMode.Wait,
        SingleReader = true,
        SingleWriter = false
    });
    internal static readonly SemaphoreSlim Semaphore = new(0);

    public static async Task PublishAsync(IChannelEvent e)
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

public class ChannelEventService(IServiceProvider provider, ILogger<ChannelEventService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var e in ChannelEvent.Channel.Reader.ReadAllAsync(stoppingToken))
        {
            await HandleEventAsync(e);
            await ChannelEvent.WaitAsync();
        }

        ChannelEvent.Channel.Writer.Complete();
        logger.LogInformation("actor event service stopped");
    }

    private async Task HandleEventAsync(IChannelEvent e)
    {
        try
        {
            await using var scope = provider.CreateAsyncScope();
            var handlers = provider.GetServices<IChannelEventHandler>();
            await Task.WhenAll(handlers
                .Where(x => x.EventType == e.GetType())
                .Select(x => x.HandleAsync(e)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "failed to handle event");
        }
    }
}