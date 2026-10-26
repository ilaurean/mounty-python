using Commons.Services;
using Inventory.Contracts;
using Inventory.Services.Features.Catalogs.Commands;
using Inventory.Tests.Infrastructure;

namespace Inventory.Tests;

internal static class BoardTestData
{
    public const string BoardID = "BRD-001";

    public static readonly CurrentUser Manager = new("manager-1", isManager: true);
    public static readonly CurrentUser Customer = new("customer-1", isManager: false);

    public static CreateBoardRequest ValidRequest(string id = BoardID) =>
        new(id, "Board", "Desc", 10m, 5m, [new ComponentRequest("Resistor", "10k resistor", 10)]);

    public static async Task<TestBoardRepository> SeedAsync(CancellationToken cancellationToken)
    {
        var repository = new TestBoardRepository();
        await new CreateBoardCommand(repository).ExecuteAsync(Manager, ValidRequest(), cancellationToken);
        return repository;
    }
}
