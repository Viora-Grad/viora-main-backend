using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Viora.Application.Abstractions.Clock;
using Viora.Domain.Abstractions;
using Viora.Domain.Scheduling;

namespace Viora.Infrastructure.Scheduling;

internal class ScheduledEventDispatcherService(
    IServiceScopeFactory scopeFactory,
    IDateTimeProvider dateTimeProvider,
    ILogger<ScheduledEventDispatcherService> log,
    ISchedulingSettings schedulingSettings)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatch(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Dispatcher batch failed");
            }

            try
            {
                await Task.Delay(schedulingSettings.PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessBatch(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = dateTimeProvider.UtcNow;

        // Only fetch the ids here; each event is then processed in its own scope so a failure in
        // one cannot poison the shared change tracker / batch save of the others.
        var dueIds = await db.Set<ScheduledDomainEvent>()
            .Where(e => e.ProcessedOn == null
                     && e.ScheduledFor <= now
                     && e.AttemptCount < schedulingSettings.MaxAttempts)
            .OrderBy(e => e.ScheduledFor)
            .Take(schedulingSettings.BatchSize)
            .Select(e => e.Id)
            .ToListAsync(ct);

        if (dueIds.Count == 0) return;

        log.LogDebug("Dispatching {Count} scheduled events", dueIds.Count);

        foreach (var id in dueIds)
            await ProcessOne(id, ct);
    }

    /// <summary>
    /// Processes a single scheduled event in its own DI scope / DbContext / transaction. Handler
    /// changes and the outbox status update commit atomically per event, so one failing event
    /// (e.g. a handler insert that violates a FK) can never fail or roll back its siblings.
    /// </summary>
    private async Task ProcessOne(Guid id, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var record = await db.Set<ScheduledDomainEvent>().FirstOrDefaultAsync(e => e.Id == id, ct);
        if (record is null) return;

        var attempt = record.AttemptCount + 1;

        try
        {
            var type = Type.GetType(record.EventType)
                ?? throw new InvalidOperationException($"Unknown event type: {record.EventType}");

            var @event = (IDomainEvent)JsonSerializer.Deserialize(record.Payload, type)!;

            await publisher.Publish(@event, ct);
            record.MarkProcessed(dateTimeProvider.UtcNow);
            await db.SaveChangesAsync(ct);

            log.LogInformation(
                "Dispatched scheduled event {Id} of type {Type}",
                record.Id, record.EventType);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Don't mark as failed on shutdown — let it retry next run.
            throw;
        }
        catch (Exception ex)
        {
            log.LogError(ex,
                "Failed to dispatch scheduled event {Id} (attempt {Attempt})",
                id, attempt);

            // Discard any partial entity changes staged by the failed handler; otherwise the same
            // bad insert would also fail the failure-bookkeeping save below.
            db.ChangeTracker.Clear();

            var fresh = await db.Set<ScheduledDomainEvent>().FirstOrDefaultAsync(e => e.Id == id, ct);
            if (fresh is null) return;

            fresh.RecordFailure(ex.Message);

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (Exception saveEx)
            {
                log.LogError(saveEx, "Failed to persist failure state for scheduled event {Id}", id);
            }
        }
    }
}