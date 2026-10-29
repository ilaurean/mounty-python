using AwesomeAssertions;
using Commons.Domain;
using Commons.Services;
using Operative.Contracts;
using Operative.Services.Features.Operations.Commands;
using Operative.Tests.Infrastructure;

namespace Operative.Tests.Commands;

public class CancelOrderCommandTests
{
    [Fact]
    public async Task It_Should_Allow_The_Owning_Customer_To_Cancel_A_Created_Order()
    {
        var repository = new TestOrderRepository();
        var created = await new CreateOrderCommand(repository).ExecuteAsync(new CurrentUser("customer-1", false), OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);

        var order = await new CancelOrderCommand(repository).ExecuteAsync(new CurrentUser("customer-1", false), created.ID, TestContext.Current.CancellationToken);

        order.Status.Should().Be(OrderStates.Cancelled);
    }

    [Fact]
    public async Task It_Should_Prevent_Another_Customer_From_Cancelling()
    {
        var repository = new TestOrderRepository();
        var created = await new CreateOrderCommand(repository).ExecuteAsync(new CurrentUser("customer-1", false), OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);

        var act = async () => await new CancelOrderCommand(repository).ExecuteAsync(new CurrentUser("customer-2", false), created.ID, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
