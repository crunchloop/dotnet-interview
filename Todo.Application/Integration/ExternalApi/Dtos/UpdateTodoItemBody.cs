using System.Text.Json.Serialization;

namespace Todo.Application.Integration.ExternalApi.Dtos;

/// <summary>Request body for PATCH /todolists/{todolistId}/todoitems/{todoitemId}.</summary>
public class UpdateTodoItemBody
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = default!;

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }
}
