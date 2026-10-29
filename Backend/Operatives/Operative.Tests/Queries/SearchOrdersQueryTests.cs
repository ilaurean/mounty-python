using AwesomeAssertions;
using Commons.Services;
using Operative.Contracts;
using Operative.Services.Features.Operations.Commands;
using Operative.Services.Features.Operations.Queries;
using Operative.Tests.Infrastructure;

namespace Operative.Tests.Queries;

public class SearchOrdersQueryTests
{
    [Fact]
    public async Task It_Should_Only_Return_Own_Orders_For_A_Customer()
    {
        var repository = new TestOrderRepository();
        await new CreateOrderCommand(repository).ExecuteAsync(new CurrentUser("customer-1", false), OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);
        await new CreateOrderCommand(repository).ExecuteAsync(new CurrentUser("customer-2", false), OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);

        var results = await new SearchOrdersQuery(repository).ExecuteAsync(new CurrentUser("customer-1", false), new OrderFilterDTO(null, null, null, null, null), TestContext.Current.CancellationToken);

        results.Should().OnlyContain(r => r.CustomerID == "customer-1");
    }

    [Fact]
    public async Task It_Should_Return_All_Orders_For_A_Manager()
    {
        var repository = new TestOrderRepository();
        await new CreateOrderCommand(repository).ExecuteAsync(new CurrentUser("customer-1", false), OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);
        await new CreateOrderCommand(repository).ExecuteAsync(new CurrentUser("customer-2", false), OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);

        var results = await new SearchOrdersQuery(repository).ExecuteAsync(new CurrentUser("manager-1", true), new OrderFilterDTO(null, null, null, null, null), TestContext.Current.CancellationToken);

        results.Should().HaveCount(2);
    }
}
