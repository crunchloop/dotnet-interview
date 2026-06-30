using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Todo.Application.BulkCompletion.Dtos;
using Todo.Application.Common.Interfaces;
using Todo.Domain.Entities;
using Todo.Infrastructure.BulkCompletion;
using Todo.Persistence;

namespace Todo.Application.Tests.BulkCompletion;

/// <summary>
/// Exercises the real background service end-to-end: it spins up a DI container with a
/// shared in-memory database so the fire-and-forget job (which opens its own scope) reads
/// the same data the test seeds. Job completion is observed by polling <c>GetProgress</c>.
/// </summary>
public class BulkCompletionServiceTests
{
    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _provider;

        public Harness(int incompleteItems, int completeItems = 0)
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<TodoContext>(options => options.UseInMemoryDatabase(dbName));
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<TodoContext>());
            _provider = services.BuildServiceProvider();

            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
            var list = new TodoList { Id = 1, Name = "List" };
            context.TodoLists.Add(list);
            for (var i = 0; i < incompleteItems; i++)
            {
                context.TodoListItems.Add(
                    new TodoListItem
                    {
                        Name = $"Incomplete {i}",
                        IsComplete = false,
                        TodoListId = 1,
                    }
                );
            }
            for (var i = 0; i < completeItems; i++)
            {
                context.TodoListItems.Add(
                    new TodoListItem
                    {
                        Name = $"Complete {i}",
                        IsComplete = true,
                        TodoListId = 1,
                    }
                );
            }
            context.SaveChanges();
        }

        public Mock<ITodoCompletionNotifier> Notifier { get; } = new();

        public BulkCompletionService CreateService() =>
            new(
                _provider.GetRequiredService<IServiceScopeFactory>(),
                Notifier.Object,
                NullLogger<BulkCompletionService>.Instance
            );

        public int CountIncomplete()
        {
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TodoContext>();
            return context.TodoListItems.Count(i => i.TodoListId == 1 && !i.IsComplete);
        }

        public void Dispose() => _provider.Dispose();
    }

    private static async Task<JobProgressDto> WaitForTerminalAsync(
        IBulkCompletionService service,
        Guid jobId
    )
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var progress = service.GetProgress(jobId);
            Assert.NotNull(progress);
            if (progress!.Status is "Completed" or "Failed" or "Cancelled")
            {
                return progress;
            }

            await Task.Delay(20);
        }

        throw new TimeoutException("Bulk completion job did not reach a terminal state in time.");
    }

    [Fact]
    public async Task StartJob_CompletesAllIncompleteItems_AndReportsCompletion()
    {
        using var harness = new Harness(incompleteItems: 5, completeItems: 2);
        var service = harness.CreateService();

        var jobId = service.StartJob(1);
        var progress = await WaitForTerminalAsync(service, jobId);

        Assert.Equal("Completed", progress.Status);
        Assert.Equal(5, progress.TotalCount);
        Assert.Equal(5, progress.CompletedCount);
        Assert.Equal(100, progress.Percentage);
        Assert.NotNull(progress.CompletedAt);
        Assert.Equal(0, harness.CountIncomplete());

        harness.Notifier.Verify(
            n =>
                n.JobStartedAsync(
                    1,
                    It.Is<JobStartedDto>(d => d.JobId == jobId && d.TotalItems == 5),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        harness.Notifier.Verify(
            n =>
                n.JobCompletedAsync(
                    1,
                    It.Is<JobCompletedDto>(d => d.JobId == jobId && d.TotalItems == 5),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task StartJob_WhenNoIncompleteItems_CompletesImmediatelyWithoutProgressUpdates()
    {
        using var harness = new Harness(incompleteItems: 0, completeItems: 3);
        var service = harness.CreateService();

        var jobId = service.StartJob(1);
        var progress = await WaitForTerminalAsync(service, jobId);

        Assert.Equal("Completed", progress.Status);
        Assert.Equal(0, progress.TotalCount);
        Assert.Equal(0, progress.CompletedCount);

        // No batch ran, so no per-batch progress update was emitted.
        harness.Notifier.Verify(
            n =>
                n.ProgressUpdatedAsync(
                    It.IsAny<long>(),
                    It.IsAny<ProgressUpdateDto>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
        harness.Notifier.Verify(
            n => n.JobCompletedAsync(1, It.IsAny<JobCompletedDto>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public void GetProgress_ForUnknownJob_ReturnsNull()
    {
        using var harness = new Harness(incompleteItems: 0);
        var service = harness.CreateService();

        Assert.Null(service.GetProgress(Guid.NewGuid()));
    }

    [Fact]
    public void CancelJob_ForUnknownJob_ReturnsFalse()
    {
        using var harness = new Harness(incompleteItems: 0);
        var service = harness.CreateService();

        Assert.False(service.CancelJob(Guid.NewGuid()));
    }
}
