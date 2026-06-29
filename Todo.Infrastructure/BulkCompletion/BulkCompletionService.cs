using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Todo.Application.BulkCompletion.Dtos;
using Todo.Application.BulkCompletion.Models;
using Todo.Application.Common.Interfaces;

namespace Todo.Infrastructure.BulkCompletion;

/// <summary>
/// Singleton that runs bulk-completion jobs on background tasks. Each job creates
/// its own DI scope (and therefore its own <see cref="IApplicationDbContext"/>),
/// processes incomplete items in fixed-size batches, and streams progress through
/// the <see cref="ITodoCompletionNotifier"/>.
/// </summary>
public class BulkCompletionService : IBulkCompletionService
{
    private const int BatchSize = 100;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITodoCompletionNotifier _notifier;
    private readonly ILogger<BulkCompletionService> _logger;

    private readonly ConcurrentDictionary<Guid, JobProgress> _jobs = new();
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _cancellations = new();

    public BulkCompletionService(
        IServiceScopeFactory scopeFactory,
        ITodoCompletionNotifier notifier,
        ILogger<BulkCompletionService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _notifier = notifier;
        _logger = logger;
    }

    public Guid StartJob(long todoListId)
    {
        var jobId = Guid.NewGuid();
        var cts = new CancellationTokenSource();

        _jobs[jobId] = new JobProgress
        {
            JobId = jobId,
            TodoListId = todoListId,
            Status = JobStatus.Pending,
            StartedAt = DateTime.UtcNow,
        };
        _cancellations[jobId] = cts;

        // Fire-and-forget: the job outlives the request that triggered it.
        _ = Task.Run(() => ProcessJobAsync(jobId, todoListId, cts.Token));

        return jobId;
    }

    public JobProgressDto? GetProgress(Guid jobId)
    {
        return _jobs.TryGetValue(jobId, out var progress) ? Map(progress) : null;
    }

    public bool CancelJob(Guid jobId)
    {
        if (!_cancellations.TryGetValue(jobId, out var cts))
        {
            return false;
        }

        cts.Cancel();
        return true;
    }

    private async Task ProcessJobAsync(Guid jobId, long todoListId, CancellationToken token)
    {
        var job = _jobs[jobId];
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var totalCount = await context.TodoListItems.CountAsync(
                item => item.TodoListId == todoListId && !item.IsComplete,
                token
            );

            job.TotalCount = totalCount;
            job.Status = JobStatus.Running;

            _logger.LogInformation(
                "Bulk completion job {JobId} started for todo list {TodoListId} with {TotalCount} incomplete item(s).",
                jobId,
                todoListId,
                totalCount
            );

            await _notifier.JobStartedAsync(
                todoListId,
                new JobStartedDto
                {
                    JobId = jobId,
                    TotalItems = totalCount,
                    Timestamp = DateTime.UtcNow,
                },
                token
            );

            while (true)
            {
                token.ThrowIfCancellationRequested();

                // Completed items drop out of this filter, so each pass naturally
                // returns the next batch until none remain.
                var batch = await context
                    .TodoListItems.Where(item => item.TodoListId == todoListId && !item.IsComplete)
                    .OrderBy(item => item.Id)
                    .Take(BatchSize)
                    .ToListAsync(token);

                if (batch.Count == 0)
                {
                    break;
                }

                var completedAt = DateTime.UtcNow;
                var completedItems = new List<CompletedItemDto>(batch.Count);

                foreach (var item in batch)
                {
                    item.IsComplete = true;
                    completedItems.Add(
                        new CompletedItemDto
                        {
                            Id = item.Id,
                            Name = item.Name,
                            IsComplete = true,
                            CompletedAt = completedAt,
                        }
                    );
                }

                await context.SaveChangesAsync(token);

                job.CompletedCount += batch.Count;

                var elapsedSeconds = Math.Max(stopwatch.Elapsed.TotalSeconds, 0.001);
                var itemsPerSecond = job.CompletedCount / elapsedSeconds;
                var remaining = totalCount - job.CompletedCount;
                var estimatedSecondsRemaining = itemsPerSecond > 0 ? remaining / itemsPerSecond : 0;

                await _notifier.ProgressUpdatedAsync(
                    todoListId,
                    new ProgressUpdateDto
                    {
                        JobId = jobId,
                        CompletedCount = job.CompletedCount,
                        TotalCount = totalCount,
                        Percentage = job.Percentage,
                        ItemsPerSecond = Math.Round(itemsPerSecond, 2),
                        EstimatedSecondsRemaining = Math.Round(estimatedSecondsRemaining, 2),
                        CompletedItems = completedItems,
                    },
                    token
                );
            }

            stopwatch.Stop();
            job.Status = JobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation(
                "Bulk completion job {JobId} completed {CompletedCount} item(s) in {Seconds}s.",
                jobId,
                job.CompletedCount,
                stopwatch.Elapsed.TotalSeconds
            );

            await _notifier.JobCompletedAsync(
                todoListId,
                new JobCompletedDto
                {
                    JobId = jobId,
                    TotalItems = job.CompletedCount,
                    CompletedAt = job.CompletedAt.Value,
                    DurationSeconds = Math.Round(stopwatch.Elapsed.TotalSeconds, 2),
                },
                // The job's own token may be cancelled; this final notification must still go out.
                CancellationToken.None
            );
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            job.Status = JobStatus.Cancelled;
            job.CompletedAt = DateTime.UtcNow;

            _logger.LogWarning(
                "Bulk completion job {JobId} was cancelled after {CompletedCount} item(s).",
                jobId,
                job.CompletedCount
            );

            await _notifier.JobCancelledAsync(
                todoListId,
                new JobCancelledDto { JobId = jobId, CancelledAt = job.CompletedAt.Value },
                CancellationToken.None
            );
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;
            job.Error = ex.Message;

            _logger.LogError(ex, "Bulk completion job {JobId} failed.", jobId);

            await _notifier.JobFailedAsync(
                todoListId,
                new JobFailedDto
                {
                    JobId = jobId,
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow,
                },
                CancellationToken.None
            );
        }
        finally
        {
            if (_cancellations.TryRemove(jobId, out var cts))
            {
                cts.Dispose();
            }
        }
    }

    private static JobProgressDto Map(JobProgress progress)
    {
        return new JobProgressDto
        {
            JobId = progress.JobId,
            TodoListId = progress.TodoListId,
            Status = progress.Status.ToString(),
            CompletedCount = progress.CompletedCount,
            TotalCount = progress.TotalCount,
            Percentage = progress.Percentage,
            StartedAt = progress.StartedAt,
            CompletedAt = progress.CompletedAt,
            Error = progress.Error,
        };
    }
}
