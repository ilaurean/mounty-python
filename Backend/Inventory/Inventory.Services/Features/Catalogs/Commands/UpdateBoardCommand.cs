using Commons.Domain;
using Commons.Services;
using Inventory.Contracts;
using Inventory.Services.Features.Catalogs.Domain;

namespace Inventory.Services.Features.Catalogs.Commands;

public interface IUpdateBoardCommand
{
    Task<Board> ExecuteAsync(CurrentUser currentUser, string boardId, UpdateBoardRequest request, CancellationToken cancellationToken);
}

public class UpdateBoardCommand(ICatalogRepository repository) : IUpdateBoardCommand
{
    public async Task<Board> ExecuteAsync(CurrentUser currentUser, string boardId, UpdateBoardRequest request, CancellationToken cancellationToken)
    {
        currentUser.EnsureManager();

        var board = await repository.GetByIDAsync(boardId, cancellationToken)
            ?? throw new NotFoundException($"Board '{boardId}' was not found.");

        board.Update(
            request.Name,
            request.Description,
            request.Length,
            request.Width,
            request.Components is null ? null : ComponentFactory.Build(request.Components));

        await repository.SaveChangesAsync(cancellationToken);
        return board;
    }
}
