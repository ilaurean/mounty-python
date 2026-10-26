using Commons.Domain;
using Commons.Services;
using Inventory.Services.Features.Catalogs.Domain;

namespace Inventory.Services.Features.Catalogs.Commands;

public interface IDeleteBoardCommand
{
    Task<Board> ExecuteAsync(CurrentUser currentUser, string boardId, CancellationToken cancellationToken);
}

public class DeleteBoardCommand(ICatalogRepository repository) : IDeleteBoardCommand
{
    public async Task<Board> ExecuteAsync(CurrentUser currentUser, string boardId, CancellationToken cancellationToken)
    {
        currentUser.EnsureManager();

        var board = await repository.GetByIDAsync(boardId, cancellationToken)
            ?? throw new NotFoundException($"Board '{boardId}' was not found.");

        board.Delete();
        await repository.SaveChangesAsync(cancellationToken);
        return board;
    }
}
