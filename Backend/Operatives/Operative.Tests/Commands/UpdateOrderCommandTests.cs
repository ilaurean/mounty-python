using AwesomeAssertions;
using Commons.Domain;
using Commons.Infrastructure.Messaging;
using Commons.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Operative.Contracts;
using Operative.Services.Features.Operations;
using Operative.Services.Features.Operations.Commands;
using Operative.Services.Features.Operations.Domain;
using Operative.Services.Infrastructure.Repositories;
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task It_Should_Save_An_Updated_Order_Item_With_Relational_Constraints(bool changeBoard)
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        var options = new DbContextOptionsBuilder<OperativeDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var context = new OperativeDbContext(options);
        await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);
        var repository = new OrderRepository(context, new Mock<IPublisher>().Object);
        var currentUser = new CurrentUser("customer-1", isManager: false);
        var order = await new CreateOrderCommand(repository).ExecuteAsync(
            currentUser,
            OrderRequestFactory.ValidRequest(),
            TestContext.Current.CancellationToken);
        var existingItem = order.Items.Single();
        var updatedQuantity = existingItem.Quantity + 3;
        var itemRequest = new CreateOrderItemRequest(
            changeBoard ? "BRD-002" : existingItem.Board.ID,
            changeBoard ? "Board 2" : existingItem.Board.Name,
            updatedQuantity,
            [new CreateOrderComponentRequest("Resistor", 10)],
            existingItem.PK);
        var request = new UpdateOrderRequest(null, null, Items: [itemRequest]);

        var updated = await new UpdateOrderCommand(repository).ExecuteAsync(
            currentUser,
            order.ID,
            request,
            TestContext.Current.CancellationToken);

        updated.Items.Single().Quantity.Should().Be(updatedQuantity);
        updated.Items.Single().Board.ID.Should().Be(itemRequest.BoardID);
        context.ChangeTracker.Clear();
        var persisted = await context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Board)
                    .ThenInclude(b => b.Components)
            .SingleAsync(o => o.ID == order.ID, TestContext.Current.CancellationToken);
        persisted.Items.Single().Quantity.Should().Be(updatedQuantity);
        persisted.Items.Single().Board.ID.Should().Be(itemRequest.BoardID);
        persisted.Items.Single().Board.Name.Should().Be(itemRequest.BoardName);
    }
}
