using System.Text.Json.Serialization;

namespace Todo.Application.Integration.ExternalApi.Dtos;

/// <summary>
/// Request body for POST /todolists. Items are created together with the list —
/// the external API has no standalone create-item endpoint.
/// </summary>
public class CreateTodoListBody
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;

    /// <summary>Our local list id, so the external API can correlate back to us.</summary>
    [JsonPropertyName("source_id")]
    public string SourceId { get; set; } = default!;

    [JsonPropertyName("items")]
    public List<CreateTodoItemBody> Items { get; set; } = new();
}

/// <summary>Item sub-body of <see cref="CreateTodoListBody"/>.</summary>
public class CreateTodoItemBody
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = default!;

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }

    /// <summary>Our local item id, used for correlation.</summary>
    [JsonPropertyName("source_id")]
    public string SourceId { get; set; } = default!;
}
