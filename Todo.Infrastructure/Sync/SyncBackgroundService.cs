using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Todo.Application.Sync.Commands.RunSync;

namespace Todo.Infrastructure.Sync;

/// <summary>
/// Periodically runs the inbound/bidirectional sync. Runs never overlap: each tick is
/// awaited to completion before the next interval, and a guard skips a tick that somehow
/// fires while a sync is still in progress.
/// </summary>
public class SyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SyncOptions _options;
    private readonly ILogger<SyncBackgroundService> _logger;

    private int _running;

    public SyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<SyncOptions> options,
        ILogger<SyncBackgroundService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Sync background worker is disabled via configuration.");
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(1, _options.IntervalSeconds));
        _logger.LogInformation(
            "Sync background worker started; interval {IntervalSeconds}s.",
            interval.TotalSeconds
        );

        using var timer = new PeriodicTimer(interval);

        do
        {
            await RunOnceAsync(stoppingToken);
        } while (
            !stoppingToken.IsCancellationRequested
            && await SafeWaitForNextTickAsync(timer, stoppingToken)
        );
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        // Guard against overlapping runs.
        if (Interlocked.CompareExchange(ref _running, 1, 0) == 1)
        {
            _logger.LogWarning("Skipping sync tick: a previous sync is still running.");
            return;
        }

        try
        {
            _logger.LogInformation("Background sync starting.");

            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var result = await mediator.Send(new RunSyncCommand(), stoppingToken);

            _logger.LogInformation(
                "Background sync finished. Lists +{LC}/~{LU}/-{LD}, Items +{IC}/~{IU}/-{ID}, errors {Errors}.",
                result.ListsCreated,
                result.ListsUpdated,
                result.ListsDeleted,
                result.ItemsCreated,
                result.ItemsUpdated,
                result.ItemsDeleted,
                result.Errors.Count
            );
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down — nothing to log as an error.
        }
        catch (ObjectDisposedException) when (stoppingToken.IsCancellationRequested)
        {
            // DI container was torn down during shutdown before cancellation propagated.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background sync failed.");
        }
        finally
        {
            Interlocked.Exchange(ref _running, 0);
        }
    }

    private static async Task<bool> SafeWaitForNextTickAsync(
        PeriodicTimer timer,
        CancellationToken stoppingToken
    )
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
