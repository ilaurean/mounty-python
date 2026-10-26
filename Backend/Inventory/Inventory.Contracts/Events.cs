using Commons.Contracts.Events;

namespace Inventory.Contracts.Events;

public record BoardCreatedEvent(BoardDTO Board) : DomainEvent<BoardDTO>();

public record BoardDeletedEvent(BoardDTO Board) : DomainEvent<BoardDTO>();

// maybe create separate events for each changeable property, but for now, we will use a single event for all changes
public record BoardUpdatedEvent(BoardDTO Board) : DomainEvent<BoardDTO>();