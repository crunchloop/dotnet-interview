namespace Todo.Infrastructure.Sync;

/// <summary>Binds the <c>Sync</c> configuration section for the background worker.</summary>
public class SyncOptions
{
    public const string SectionName = "Sync";

    /// <summary>How often the background worker runs a sync. Default 60s.</summary>
    public int IntervalSeconds { get; set; } = 60;

    /// <summary>Whether the background worker is enabled at all.</summary>
    public bool Enabled { get; set; } = true;
}
