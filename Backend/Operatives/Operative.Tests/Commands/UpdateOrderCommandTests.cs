using AwesomeAssertions;
using Commons.Domain;
using Commons.Services;
using Operative.Contracts;
using Operative.Services.Features.Operations.Commands;
using Operative.Services.Features.Operations.Domain;
using Operative.Tests.Infrastructure;

namespace Operative.Tests.Commands;

public class UpdateOrderCommandTests
{
    private static async Task<Order> SeedOrderAsync(TestOrderRepository repository, string customerId)
    {
        var command = new CreateOrderCommand(repository);
        var currentUser = new CurrentUser(customerId, isManager: false);
        return await command.ExecuteAsync(currentUser, OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task It_Should_Update_Name_And_Description_For_The_Owning_Customer()
    {
        var repository = new TestOrderRepository();
        var order = await SeedOrderAsync(repository, "customer-1");
        var command = new UpdateOrderCommand(repository);
        var currentUser = new CurrentUser("customer-1", isManager: false);

        var updated = await command.ExecuteAsync(currentUser, order.ID, new UpdateOrderRequest("New name", "New description"), TestContext.Current.CancellationToken);

        updated.Name.Should().Be("New name");
        updated.Description.Should().Be("New description");
    }

    [Fact]
    public async Task It_Should_Throw_NotFound_When_Another_Customer_Attempts_To_Update()
    {
        var repository = new TestOrderRepository();
        var order = await SeedOrderAsync(repository, "customer-1");
        var command = new UpdateOrderCommand(repository);
        var currentUser = new CurrentUser("customer-2", isManager: false);

        var act = async () => await command.ExecuteAsync(currentUser, order.ID, new UpdateOrderRequest("New name", null), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task It_Should_Throw_NotFound_When_Order_Does_Not_Exist()
    {
        var repository = new TestOrderRepository();
        var command = new UpdateOrderCommand(repository);
        var currentUser = new CurrentUser("customer-1", isManager: false);

        var act = async () => await command.ExecuteAsync(currentUser, "missing", new UpdateOrderRequest("x", null), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
