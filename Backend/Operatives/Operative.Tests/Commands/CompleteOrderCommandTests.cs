using AwesomeAssertions;
using Commons.Services;
using Operative.Contracts;
using Operative.Services.Features.Operations.Commands;
using Operative.Tests.Infrastructure;

namespace Operative.Tests.Commands;

public class CompleteOrderCommandTests
{
    [Fact]
    public async Task It_Should_Complete_A_Processing_Order()
    {
        var repository = new TestOrderRepository();
        var created = await new CreateOrderCommand(repository).ExecuteAsync(new CurrentUser("customer-1", false), OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);
        await new StartOrderProcessingCommand(repository).ExecuteAsync(new CurrentUser("manager-1", true), created.ID, TestContext.Current.CancellationToken);

        var order = await new CompleteOrderCommand(repository).ExecuteAsync(new CurrentUser("manager-1", true), created.ID, TestContext.Current.CancellationToken);

        order.Status.Should().Be(OrderStates.Completed);
    }
}
