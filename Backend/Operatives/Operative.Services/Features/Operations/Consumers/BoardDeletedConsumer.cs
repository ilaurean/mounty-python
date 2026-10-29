using Inventory.Contracts.Events;
using Rebus.Handlers;
using ILogger = Serilog.ILogger;

namespace Operative.Services.Features.Operations.Consumers;

public interface IBoardDeletedConsumer : IHandleMessages<BoardDeletedEvent>
{
}

public class BoardDeletedConsumer(ILogger logger) : IBoardDeletedConsumer
{
    public Task Handle(BoardDeletedEvent message)
    {
        logger.Information("Board with ID: {BoardId} deleted event received.", message.Board.ID);
        return Task.CompletedTask;
    }
}
