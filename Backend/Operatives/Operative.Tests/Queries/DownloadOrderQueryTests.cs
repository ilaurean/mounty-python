using AwesomeAssertions;
using Commons.Services;
using Operative.Services.Features.Operations.Commands;
using Operative.Services.Features.Operations.Queries;
using Operative.Tests.Infrastructure;

namespace Operative.Tests.Queries;

public class DownloadOrderQueryTests
{
    [Fact]
    public async Task It_Should_Return_A_Json_File_For_The_Owning_Customer()
    {
        var repository = new TestOrderRepository();
        var created = await new CreateOrderCommand(repository).ExecuteAsync(new CurrentUser("customer-1", false), OrderRequestFactory.ValidRequest(), TestContext.Current.CancellationToken);

        var download = await new DownloadOrderQuery(repository).ExecuteAsync(new CurrentUser("customer-1", false), created.ID, TestContext.Current.CancellationToken);

        download.ContentType.Should().Be("application/json");
        download.Content.Should().NotBeEmpty();
    }
}
