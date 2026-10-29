using Inventory.Contracts.Events;
using Notification.Services.Infrastructure.Messaging;
using Rebus.Handlers;
using ILogger = Serilog.ILogger;

namespace Notification.Services.Consumers;

public class BoardConsumer(IPushNotificationService pushService, ILogger logger) :
    IHandleMessages<BoardCreatedEvent>,
    IHandleMessages<BoardUpdatedEvent>,
    IHandleMessages<BoardDeletedEvent>
{
    public async Task Handle(BoardCreatedEvent boardEvent)
    {
        logger.Information("Board with ID: {BoardId} created.", boardEvent.Board.ID);
        await pushService.UpdatedBoard(boardEvent.Board);
    }

    public async Task Handle(BoardUpdatedEvent boardEvent)
    {
        logger.Information("Board with ID: {BoardId} updated.", boardEvent.Board.ID);
        await pushService.UpdatedBoard(boardEvent.Board);
    }

    public async Task Handle(BoardDeletedEvent boardEvent)
    {
        logger.Information("Board with ID: {BoardId} deleted.", boardEvent.Board.ID);
        await pushService.UpdatedBoard(boardEvent.Board);
    }
}
