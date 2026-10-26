using AwesomeAssertions;
using Commons.Services;
using Operative.Services.Features.Operations.Commands;
using Operative.Tests.Infrastructure;

namespace Operative.Tests.Commands;

public class CreateOrderCommandTests
{
    [Fact]
    public async Task It_Should_Create_An_Order_Owned_By_The_Current_User()
    {
        var repository = new TestOrderRepository();
        var command = new CreateOrderCommand(repository);
        var currentUser = new CurrentUser("customer-1", isManager: false);

        var order = await command.ExecuteAsync(currentUser, OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);

        order.CustomerID.Should().Be("customer-1");
        order.Items.Should().HaveCount(1);
        order.Items[0].Board.Components.Should().ContainSingle(c => c.Name == "Resistor");
    }

    [Fact]
    public async Task It_Should_Throw_When_Request_Is_Invalid()
    {
        var repository = new TestOrderRepository();
        var command = new CreateOrderCommand(repository);
        var currentUser = new CurrentUser("customer-1", isManager: false);
        var request = OrderRequestFactory.ValidRequest() with { Name = string.Empty };

        var act = async () => await command.ExecuteAsync(currentUser, request, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
