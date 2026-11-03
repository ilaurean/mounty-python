using Inventory.Contracts.Events;
using Operative.Contracts.Events;
using Rebus.Bus;
using Rebus.Handlers;

namespace Notification.Services.Consumers;

public static class ConsumerExtensions
{
    public static IServiceCollection AddMessageHandlers(this IServiceCollection services)
    {
        services.AddTransient<IHandleMessages<BoardCreatedEvent>, BoardConsumer>();
        services.AddTransient<IHandleMessages<BoardUpdatedEvent>, BoardConsumer>();
        services.AddTransient<IHandleMessages<BoardDeletedEvent>, BoardConsumer>();

        services.AddTransient<IHandleMessages<OrderCreatedEvent>, OrderConsumer>();
        services.AddTransient<IHandleMessages<OrderUpdatedNameOrDescriptionEvent>, OrderConsumer>();
        services.AddTransient<IHandleMessages<OrderStartProcessingEvent>, OrderConsumer>();
        services.AddTransient<IHandleMessages<OrderCompletedEvent>, OrderConsumer>();
        services.AddTransient<IHandleMessages<OrderCancelledEvent>, OrderConsumer>();
        services.AddTransient<IHandleMessages<OrderDeletedEvent>, OrderConsumer>();

        return services;
    }

    public static async Task SubscribeToMessagesAsync(IBus bus)
    {
        await bus.Subscribe<BoardCreatedEvent>();
        await bus.Subscribe<BoardUpdatedEvent>();
        await bus.Subscribe<BoardDeletedEvent>();
        await bus.Subscribe<OrderCreatedEvent>();
        await bus.Subscribe<OrderUpdatedNameOrDescriptionEvent>();
        await bus.Subscribe<OrderStartProcessingEvent>();
        await bus.Subscribe<OrderCompletedEvent>();
        await bus.Subscribe<OrderCancelledEvent>();
        await bus.Subscribe<OrderDeletedEvent>();
    }
}
