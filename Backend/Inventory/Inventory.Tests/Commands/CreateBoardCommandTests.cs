using AwesomeAssertions;
using Inventory.Contracts.Events;
using Inventory.Services.Features.Catalogs.Commands;
using Inventory.Tests.Infrastructure;
using Moq;
using static Inventory.Tests.BoardTestData;

namespace Inventory.Tests.Commands;

public class CreateBoardCommandTests
{
    [Fact]
    public async Task Manager_Can_Create_Board_And_Event_Is_Published()
    {
        var repository = await SeedAsync(TestContext.Current.CancellationToken);

        repository.Entities.Should().ContainSingle(b => b.ID == BoardID);
        repository.VerifyPublished<BoardCreatedEvent>(Times.Once());
    }

    [Fact]
    public async Task Customer_Cannot_Create_Board()
    {
        var repository = new TestBoardRepository();

        var act = async () => await new CreateBoardCommand(repository).ExecuteAsync(Customer, ValidRequest(), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        repository.VerifyPublished<BoardCreatedEvent>(Times.Never());
    }

    [Fact]
    public async Task Create_Rejects_Invalid_Request()
    {
        var repository = new TestBoardRepository();

        var act = async () => await new CreateBoardCommand(repository).ExecuteAsync(Manager, ValidRequest() with { Name = "" }, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
