using Notification.Services.Infrastructure.Messaging;
using Operative.Contracts.Events;
using Rebus.Handlers;
using ILogger = Serilog.ILogger;

namespace Notification.Services.Consumers;

public class OrderCreatedConsumer(IPushNotificationService pushService, ILogger logger) :
    IHandleMessages<OrderCreatedEvent>,
    IHandleMessages<OrderUpdatedNameOrDescriptionEvent>,
    IHandleMessages<OrderStartProcessingEvent>,
    IHandleMessages<OrderCompletedEvent>,
    IHandleMessages<OrderCancelledEvent>
{
    public async Task Handle(OrderCreatedEvent orderEvent)
    {
        logger.Information("New order created with ID: {OrderId}.", orderEvent.Order.ID);
        await pushService.UpdatedOrder(orderEvent.Order);
    }

    public async Task Handle(OrderUpdatedNameOrDescriptionEvent orderEvent)
    {
        logger.Information("Order with ID: {OrderId} updated its name or description.", orderEvent.Order.ID);
        await pushService.UpdatedOrder(orderEvent.Order);
    }

    public async Task Handle(OrderStartProcessingEvent orderEvent)
    {
        logger.Information("Order with ID: {OrderId} started processing.", orderEvent.Order.ID);
        await pushService.UpdatedOrder(orderEvent.Order);
    }

    public async Task Handle(OrderCompletedEvent orderEvent)
    {
        logger.Information("Order with ID: {OrderId} completed.", orderEvent.Order.ID);
        await pushService.UpdatedOrder(orderEvent.Order);
    }

    public async Task Handle(OrderCancelledEvent orderEvent)
    {
        logger.Information("Order with ID: {OrderId} cancelled.", orderEvent.Order.ID);
        await pushService.UpdatedOrder(orderEvent.Order);
    }
}
