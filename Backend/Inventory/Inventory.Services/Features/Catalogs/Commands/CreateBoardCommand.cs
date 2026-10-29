using Commons.Services;
using Inventory.Contracts;
using Inventory.Services.Features.Catalogs.Domain;

namespace Inventory.Services.Features.Catalogs.Commands;

public interface ICreateBoardCommand
{
    Task<Board> ExecuteAsync(CurrentUser currentUser, CreateBoardRequest request, CancellationToken cancellationToken);
}

public class CreateBoardCommand(ICatalogRepository repository) : ICreateBoardCommand
{
    public async Task<Board> ExecuteAsync(CurrentUser currentUser, CreateBoardRequest request, CancellationToken cancellationToken)
    {
        currentUser.EnsureManager();

        var board = Board.Create
            .WithID(request.ID)
            .WithName(request.Name)
            .WithDescription(request.Description!)
            .WithDimension(request.Length, request.Width)
            .WithComponents(ComponentFactory.Build(request.Components))
            .Build();

        await repository.AddAsync(board, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return board;
    }
}
