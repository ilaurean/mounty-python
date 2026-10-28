using Commons.Domain;
using Commons.Services;
using Inventory.Contracts;
using Inventory.Services.Features.Catalogs.Domain;
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
            request.Components is null ? null : MergeComponents(board, request.Components));

        await repository.SaveChangesAsync(cancellationToken);
        return board;
    }

    private static IList<Component> MergeComponents(Board board, IEnumerable<ComponentRequest> requests)
    {
        var result = new List<Component>();
        foreach (var request in requests)
        {
            var existing = request.PK is null ? null : board.Components.FirstOrDefault(c => c.PK == request.PK);
            if (existing is null)
            {
                result.AddRange(ComponentFactory.Build([request]));
                continue;
            }

            existing.Update(request.Name, request.Description, request.Quantity);
            result.Add(existing);
        }
        return result;
    }
}
