using System.Text.Json.Serialization;

namespace Todo.Application.Integration.ExternalApi.Dtos;

/// <summary>Request body for PATCH /todolists/{id}.</summary>
public class UpdateTodoListBody
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
}
