using AwesomeAssertions;
using Inventory.Services.Features.Catalogs.Queries;
using static Inventory.Tests.BoardTestData;

namespace Inventory.Tests.Queries;

public class GetBoardByIDQueryTests
{
    [Fact]
    public async Task Customer_Can_Get_Board()
    {
        var repository = await SeedAsync(TestContext.Current.CancellationToken);

        var dto = await new GetBoardByIDQuery(repository).ExecuteAsync(Customer, BoardID, TestContext.Current.CancellationToken);

        dto.Components.Should().HaveCount(1);
    }
}
