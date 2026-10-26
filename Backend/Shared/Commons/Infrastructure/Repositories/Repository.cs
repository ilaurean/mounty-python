using Commons.Contracts.Events;
using Commons.Domain;
using Commons.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Commons.Infrastructure.Repositories;

public interface IRepository<TEntity> where TEntity : AuditableEntity
{
    IQueryable<TEntity> Entities { get; }
    Task<TEntity?> GetByIDAsync(string id, CancellationToken cancellationToken);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public abstract class Repository<TEntity, TContext>(TContext context, IPublisher publisher) : IRepository<TEntity>
    where TEntity : AuditableEntity
    where TContext : DbContext
{
    protected TContext Context => context;

    public abstract IQueryable<TEntity> Entities { get; }

    public abstract Task<TEntity?> GetByIDAsync(string id, CancellationToken cancellationToken);

    public abstract Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        var events = new List<DomainEvent>();
        var auditableEntities = context.ChangeTracker.Entries()
            .Select(e => e.Entity)
            .OfType<AuditableEntity>();
        foreach (var e in auditableEntities)
        {
            events.AddRange(e.Events);
            e.Events.Clear();
        }

        await context.SaveChangesAsync(cancellationToken);

        foreach (var e in events)
        {
            await publisher.PublishAsync(e, cancellationToken);
        }
    }
}