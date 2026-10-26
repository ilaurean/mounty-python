using Commons.Contracts.Events;
using Rebus.Bus;

namespace Commons.Infrastructure.Messaging;

public interface IPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : DomainEvent;
}

public class Publisher(IBus bus) : IPublisher
{
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : DomainEvent
    {
        cancellationToken.ThrowIfCancellationRequested();
        await bus.Publish(@event);
    }
}
