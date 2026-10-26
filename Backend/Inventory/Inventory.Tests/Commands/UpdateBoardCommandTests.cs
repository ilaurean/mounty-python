using AwesomeAssertions;
using Commons.Domain;
using Inventory.Contracts;
using Inventory.Contracts.Events;
using Inventory.Services.Features.Catalogs.Commands;
using Inventory.Tests.Infrastructure;
using Moq;
using static Inventory.Tests.BoardTestData;

namespace Inventory.Tests.Commands;

public class UpdateBoardCommandTests
{
    [Fact]
    public async Task Manager_Can_Update_Board()
    {
        var repository = await SeedAsync(TestContext.Current.CancellationToken);

        var board = await new UpdateBoardCommand(repository).ExecuteAsync(Manager, BoardID, new UpdateBoardRequest("New", "D", 1m, 2m, null), TestContext.Current.CancellationToken);

        board.Name.Should().Be("New");
        repository.VerifyPublished<BoardUpdatedEvent>(Times.AtLeastOnce());
    }

    [Fact]
    public async Task Customer_Cannot_Update_Board()
    {
        var repository = await SeedAsync(TestContext.Current.CancellationToken);

        var act = async () => await new UpdateBoardCommand(repository).ExecuteAsync(Customer, BoardID, new UpdateBoardRequest("New", null, null, null, null), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Update_Missing_Board_Throws_NotFound()
    {
        var repository = new TestBoardRepository();

        var act = async () => await new UpdateBoardCommand(repository).ExecuteAsync(Manager, "NOPE", new UpdateBoardRequest("New", null, null, null, null), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
