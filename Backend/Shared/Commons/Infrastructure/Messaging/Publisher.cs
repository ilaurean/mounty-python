using Commons.Domain;
using Commons.Contracts.Events;
using Rebus.Bus;
using Serilog;

namespace Commons.Infrastructure.Messaging;

public interface IPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : DomainEvent;
}

public class Publisher(IBus bus, ILogger logger) : IPublisher
{
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : DomainEvent
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.Information("Publishing event {EventType}", typeof(T).FriendlyName());
        logger.Verbose("Event details: {@Event}", @event);
        await bus.Publish(@event);
    }
}
