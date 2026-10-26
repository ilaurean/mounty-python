using Inventory.Contracts.Events;
using Notification.Services.Infrastructure.Messaging;
using Rebus.Handlers;
using ILogger = Serilog.ILogger;

namespace Notification.Services.Consumers;

public class BoardConsumer(IPushNotificationService pushService, ILogger logger) : IHandleMessages<BoardCreatedEvent>, IHandleMessages<BoardDeletedEvent>
{
    public async Task Handle(BoardCreatedEvent boardEvent)
    {
        logger.Information("New board created with ID: {BoardId}.", boardEvent.Board.ID);
        await pushService.UpdatedBoard(boardEvent.Board);
    }

    public async Task Handle(BoardDeletedEvent boardEvent)
    {
        logger.Information("Board with ID: {BoardId} deleted.", boardEvent.Board.ID);
        await pushService.UpdatedBoard(boardEvent.Board);
    }
}
