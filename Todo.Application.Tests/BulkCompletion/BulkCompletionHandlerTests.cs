using Microsoft.EntityFrameworkCore;
using Moq;
using Todo.Application.BulkCompletion.Commands.CancelBulkCompletion;
using Todo.Application.BulkCompletion.Commands.StartBulkCompletion;
using Todo.Application.BulkCompletion.Dtos;
using Todo.Application.BulkCompletion.Queries.GetBulkJobProgress;
using Todo.Application.Common.Exceptions;
using Todo.Application.Common.Interfaces;
using Todo.Domain.Entities;
using Todo.Persistence;

namespace Todo.Application.Tests.BulkCompletion;

public class BulkCompletionHandlerTests
{
    private static DbContextOptions<TodoContext> DatabaseContextOptions() =>
        new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    private static TodoContext ContextWithList()
    {
        var context = new TodoContext(DatabaseContextOptions());
        context.TodoLists.Add(new TodoList { Id = 1, Name = "Task 1" });
        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task StartBulkCompletion_WhenTodoListDoesntExist_ThrowsNotFoundException()
    {
        using var context = ContextWithList();
        var service = new Mock<IBulkCompletionService>();

        var handler = new StartBulkCompletionCommandHandler(context, service.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () =>
                handler.Handle(
                    new StartBulkCompletionCommand { TodoListId = 99 },
                    CancellationToken.None
                )
        );

        // No job should be queued for a non-existent list.
        service.Verify(s => s.StartJob(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task StartBulkCompletion_WhenListExists_StartsJobAndReturnsItsId()
    {
        using var context = ContextWithList();
        var jobId = Guid.NewGuid();
        var service = new Mock<IBulkCompletionService>();
        service.Setup(s => s.StartJob(1)).Returns(jobId);

        var handler = new StartBulkCompletionCommandHandler(context, service.Object);

        var result = await handler.Handle(
            new StartBulkCompletionCommand { TodoListId = 1 },
            CancellationToken.None
        );

        Assert.Equal(jobId, result);
        service.Verify(s => s.StartJob(1), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CancelBulkCompletion_ReturnsServiceResult(bool serviceResult)
    {
        var jobId = Guid.NewGuid();
        var service = new Mock<IBulkCompletionService>();
        service.Setup(s => s.CancelJob(jobId)).Returns(serviceResult);

        var handler = new CancelBulkCompletionCommandHandler(service.Object);

        var result = await handler.Handle(
            new CancelBulkCompletionCommand { JobId = jobId },
            CancellationToken.None
        );

        Assert.Equal(serviceResult, result);
        service.Verify(s => s.CancelJob(jobId), Times.Once);
    }

    [Fact]
    public async Task GetBulkJobProgress_WhenJobKnown_ReturnsSnapshot()
    {
        var jobId = Guid.NewGuid();
        var snapshot = new JobProgressDto
        {
            JobId = jobId,
            TodoListId = 1,
            Status = "Running",
            CompletedCount = 3,
            TotalCount = 10,
            Percentage = 30,
            StartedAt = DateTime.UtcNow,
        };
        var service = new Mock<IBulkCompletionService>();
        service.Setup(s => s.GetProgress(jobId)).Returns(snapshot);

        var handler = new GetBulkJobProgressQueryHandler(service.Object);

        var result = await handler.Handle(
            new GetBulkJobProgressQuery { JobId = jobId },
            CancellationToken.None
        );

        Assert.Same(snapshot, result);
    }

    [Fact]
    public async Task GetBulkJobProgress_WhenJobUnknown_ReturnsNull()
    {
        var service = new Mock<IBulkCompletionService>();
        service.Setup(s => s.GetProgress(It.IsAny<Guid>())).Returns((JobProgressDto?)null);

        var handler = new GetBulkJobProgressQueryHandler(service.Object);

        var result = await handler.Handle(
            new GetBulkJobProgressQuery { JobId = Guid.NewGuid() },
            CancellationToken.None
        );

        Assert.Null(result);
    }
}
