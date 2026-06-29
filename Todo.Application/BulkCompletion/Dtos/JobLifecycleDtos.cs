namespace Todo.Application.BulkCompletion.Dtos;

/// <summary>Broadcast as the "JobStarted" SignalR message.</summary>
public class JobStartedDto
{
    public Guid JobId { get; set; }
    public int TotalItems { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>Broadcast as the "JobCompleted" SignalR message.</summary>
public class JobCompletedDto
{
    public Guid JobId { get; set; }
    public int TotalItems { get; set; }
    public DateTime CompletedAt { get; set; }
    public double DurationSeconds { get; set; }
}

/// <summary>Broadcast as the "JobFailed" SignalR message.</summary>
public class JobFailedDto
{
    public Guid JobId { get; set; }
    public required string Error { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>Broadcast as the "JobCancelled" SignalR message.</summary>
public class JobCancelledDto
{
    public Guid JobId { get; set; }
    public DateTime CancelledAt { get; set; }
}
