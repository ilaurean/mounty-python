namespace Commons.Contracts.Events;

public abstract record DomainEvent<T>() : DomainEvent where T : AuditableEntityDTO;

public abstract record DomainEvent();
