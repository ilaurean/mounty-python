using AwesomeAssertions;
using Commons.Domain;
using Inventory.Contracts;
using Inventory.Contracts.Events;
using Inventory.Services.Features.Catalogs.Commands;
using Inventory.Services.Features.Catalogs.Queries;
using Moq;
using static Inventory.Tests.BoardTestData;

namespace Inventory.Tests.Commands;

public class DeleteBoardCommandTests
{
    [Fact]
    public async Task Manager_Can_Delete_Board_And_It_Is_Hidden_From_Queries()
    {
        var repository = await SeedAsync(TestContext.Current.CancellationToken);

        await new DeleteBoardCommand(repository).ExecuteAsync(Manager, BoardID, TestContext.Current.CancellationToken);

        repository.VerifyPublished<BoardDeletedEvent>(Times.AtLeastOnce());
        var act = async () => await new GetBoardByIDQuery(repository).ExecuteAsync(Customer, BoardID, TestContext.Current.CancellationToken);
        await act.Should().ThrowAsync<NotFoundException>();
        (await new SearchBoardsQuery(repository).ExecuteAsync(Customer, new BoardFilterDTO(null, null), TestContext.Current.CancellationToken)).Should().BeEmpty();
    }

    [Fact]
    public async Task Customer_Cannot_Delete_Board()
    {
        var repository = await SeedAsync(TestContext.Current.CancellationToken);

        var act = async () => await new DeleteBoardCommand(repository).ExecuteAsync(Customer, BoardID, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
