using System.Text.Json.Serialization;

namespace Todo.Application.Integration.ExternalApi.Dtos;

/// <summary>
/// TodoItem as returned by the external API (response model).
/// </summary>
public class ExternalTodoItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>The id from the source system — our local item id.</summary>
    [JsonPropertyName("source_id")]
    public string? SourceId { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = default!;

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
