using Inventory.Contracts.Events;
using Rebus.Handlers;
using ILogger = Serilog.ILogger;

namespace Operative.Services.Features.Operations.Consumers;

public interface IBoardUpdatedConsumer : IHandleMessages<BoardUpdatedEvent>
{
}

public class BoardUpdatedConsumer(ILogger logger) : IBoardUpdatedConsumer
{
    public Task Handle(BoardUpdatedEvent message)
    {
        logger.Information("Board with ID: {BoardId} updated event received.", message.Board.ID);
        return Task.CompletedTask;
    }
}
