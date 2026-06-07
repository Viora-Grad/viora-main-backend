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
        var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        var now = dateTimeProvider.UtcNow;

        var due = await db.Set<ScheduledDomainEvent>()
            .Where(e => e.ProcessedOn == null
                     && e.ScheduledFor <= now
                     && e.AttemptCount < schedulingSettings.MaxAttempts)
            .OrderBy(e => e.ScheduledFor)
            .Take(schedulingSettings.BatchSize)
            .ToListAsync(ct);

        if (due.Count == 0) return;

        log.LogDebug("Dispatching {Count} scheduled events", due.Count);

        foreach (var record in due)
        {
            try
            {
                var type = Type.GetType(record.EventType)
                    ?? throw new InvalidOperationException(
                        $"Unknown event type: {record.EventType}");

                var @event = (IDomainEvent)JsonSerializer.Deserialize(record.Payload, type)!;

                await publisher.Publish(@event, ct);
                record.MarkProcessed(dateTimeProvider.UtcNow);

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
                    record.Id, record.AttemptCount + 1);
                record.RecordFailure(ex.Message);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}