using AwesomeAssertions;
using Commons.Domain;
using Commons.Services;
using Operative.Services.Features.Operations.Commands;
using Operative.Services.Features.Operations.Queries;
using Operative.Tests.Infrastructure;

namespace Operative.Tests.Queries;

public class GetOrderByIDQueryTests
{
    [Fact]
    public async Task It_Should_Return_The_Order_For_The_Owning_Customer()
    {
        var repository = new TestOrderRepository();
        var created = await new CreateOrderCommand(repository).ExecuteAsync(
            new CurrentUser("customer-1", false), OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);

        var dto = await new GetOrderByIDQuery(repository).ExecuteAsync(new CurrentUser("customer-1", false), created.ID, TestContext.Current.CancellationToken);

        dto.ID.Should().Be(created.ID);
    }

    [Fact]
    public async Task It_Should_Throw_NotFound_For_Another_Customer()
    {
        var repository = new TestOrderRepository();
        var created = await new CreateOrderCommand(repository).ExecuteAsync(
            new CurrentUser("customer-1", false), OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);

        var act = async () => await new GetOrderByIDQuery(repository).ExecuteAsync(new CurrentUser("customer-2", false), created.ID, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task It_Should_Allow_Managers_To_View_Any_Order()
    {
        var repository = new TestOrderRepository();
        var created = await new CreateOrderCommand(repository).ExecuteAsync(
            new CurrentUser("customer-1", false), OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);

        var dto = await new GetOrderByIDQuery(repository).ExecuteAsync(new CurrentUser("manager-1", true), created.ID, TestContext.Current.CancellationToken);

        dto.ID.Should().Be(created.ID);
    }
}
