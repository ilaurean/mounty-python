using Commons.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Moq;
using Operative.Services.Features.Operations;
using Operative.Services.Infrastructure.Repositories;

namespace Operative.Tests.Infrastructure;

/// <summary>Real OrderRepository backed by an isolated EF Core InMemory database and a mocked publisher.</summary>
public sealed class TestOrderRepository() : OrderRepository(CreateContext(), new Mock<IPublisher>().Object)
{
    private static OperativeDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<OperativeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);
}
