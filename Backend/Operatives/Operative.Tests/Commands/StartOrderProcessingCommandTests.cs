using AwesomeAssertions;
using Commons.Services;
using Operative.Contracts;
using Operative.Services.Features.Operations.Commands;
using Operative.Tests.Infrastructure;

namespace Operative.Tests.Commands;

public class StartOrderProcessingCommandTests
{
    [Fact]
    public async Task It_Should_Allow_Manager_To_Start_Processing()
    {
        var repository = new TestOrderRepository();
        var created = await new CreateOrderCommand(repository).ExecuteAsync(new CurrentUser("customer-1", false), OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);
        var command = new StartOrderProcessingCommand(repository);

        var order = await command.ExecuteAsync(new CurrentUser("manager-1", isManager: true), created.ID, TestContext.Current.CancellationToken);

        order.Status.Should().Be(OrderStates.Processing);
    }

    [Fact]
    public async Task It_Should_Reject_Non_Managers()
    {
        var repository = new TestOrderRepository();
        var created = await new CreateOrderCommand(repository).ExecuteAsync(new CurrentUser("customer-1", false), OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);
        var command = new StartOrderProcessingCommand(repository);

        var act = async () => await command.ExecuteAsync(new CurrentUser("customer-1", isManager: false), created.ID, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}
