using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;
using Viora.Application.Abstractions.Clock;
using Viora.Application.Abstractions.Exceptions;
using Viora.Domain.Abstractions;


namespace Viora.Infrastructure;

internal class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IPublisher _publisher;

    public ApplicationDbContext(
        DbContextOptions options,
        IDateTimeProvider dateTimeProvider,
        IPublisher publisher
        ) : base(options)
    {
        _publisher = publisher;
        _dateTimeProvider = dateTimeProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await PuplishDomainEventAsync(cancellationToken);

            var result = await base.SaveChangesAsync(cancellationToken);


            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Concurrency exception occurred.", ex);

        }
    }

    public async Task PuplishDomainEventAsync(CancellationToken cancellationToken)
    {
        var domainEntities = ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents != null && e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();
        var domainEvents = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();
        ChangeTracker.Entries<Entity>()
        .ToList()
        .ForEach(e => e.Entity.ClearDomainEvents());
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }

    }

}