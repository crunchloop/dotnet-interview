using Todo.Application.BulkCompletion.Models;

namespace Todo.Application.Tests.BulkCompletion;

public class JobProgressTests
{
    [Fact]
    public void Percentage_WhenTotalIsZero_Is100()
    {
        // An empty list is "fully complete" by definition; avoids a divide-by-zero.
        var progress = new JobProgress { TotalCount = 0, CompletedCount = 0 };

        Assert.Equal(100, progress.Percentage);
    }

    [Fact]
    public void Percentage_WhenNoneCompleted_IsZero()
    {
        var progress = new JobProgress { TotalCount = 10, CompletedCount = 0 };

        Assert.Equal(0, progress.Percentage);
    }

    [Fact]
    public void Percentage_WhenAllCompleted_Is100()
    {
        var progress = new JobProgress { TotalCount = 10, CompletedCount = 10 };

        Assert.Equal(100, progress.Percentage);
    }

    [Fact]
    public void Percentage_RoundsToTwoDecimals()
    {
        // 1/3 -> 33.333... rounded to 33.33.
        var progress = new JobProgress { TotalCount = 3, CompletedCount = 1 };

        Assert.Equal(33.33, progress.Percentage);
    }
}
