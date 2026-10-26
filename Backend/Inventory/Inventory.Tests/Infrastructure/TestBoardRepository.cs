using Commons.Contracts.Events;
using Commons.Infrastructure.Messaging;
using Inventory.Services.Features.Catalogs;
using Inventory.Services.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Inventory.Tests.Infrastructure;

/// <summary>Real BoardRepository backed by an isolated EF Core InMemory database and a mocked publisher.</summary>
public sealed class TestBoardRepository : CatalogRepository
{
    private TestBoardRepository(Mock<IPublisher> publisher) : base(CreateContext(), publisher.Object)
    {
        Publisher = publisher;
    }

    public TestBoardRepository() : this(new Mock<IPublisher>())
    {
    }

    public Mock<IPublisher> Publisher { get; }

    public void VerifyPublished<TEvent>(Times times) where TEvent : DomainEvent =>
        Publisher.Verify(
            p => p.PublishAsync(It.Is<DomainEvent>(e => e is TEvent), It.IsAny<CancellationToken>()),
            times);

    private static InventoryDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
