using Commons.Contracts.Events;

namespace Operative.Contracts.Events;

public record OrderCreatedEvent(OrderDTO Order) : DomainEvent<OrderDTO>();
public record OrderUpdatedNameOrDescriptionEvent(OrderDTO Order) : DomainEvent<OrderDTO>();
public record OrderStartProcessingEvent(OrderDTO Order) : DomainEvent<OrderDTO>();
public record OrderCompletedEvent(OrderDTO Order) : DomainEvent<OrderDTO>();
public record OrderCancelledEvent(OrderDTO Order) : DomainEvent<OrderDTO>();
