using System.Text.Json.Serialization;

namespace Todo.Application.Integration.ExternalApi.Dtos;

/// <summary>
/// TodoList as returned by the external API (response model).
/// </summary>
public class ExternalTodoList
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    /// <summary>The id from the source system — our local list id.</summary>
    [JsonPropertyName("source_id")]
    public string? SourceId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("items")]
    public List<ExternalTodoItem> Items { get; set; } = new();
}
