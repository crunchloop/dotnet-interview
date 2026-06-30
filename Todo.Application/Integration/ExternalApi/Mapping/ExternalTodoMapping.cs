using System.Globalization;
using Todo.Application.Integration.ExternalApi.Dtos;
using Todo.Domain.Entities;

namespace Todo.Application.Integration.ExternalApi.Mapping;

/// <summary>
/// Manual mapping between local domain entities and external API DTOs, matching the
/// project's no-AutoMapper convention. Local <c>Name</c> maps to the external
/// <c>description</c>; correlation uses the local id as the external <c>source_id</c>.
/// </summary>
public static class ExternalTodoMapping
{
    public static string ToSourceId(this long localId) =>
        localId.ToString(CultureInfo.InvariantCulture);

    /// <summary>Build the POST /todolists body for a local list and its items.</summary>
    public static CreateTodoListBody ToCreateBody(this TodoList list) =>
        new()
        {
            Name = list.Name,
            SourceId = list.Id.ToSourceId(),
            Items = list.Items.Select(item => item.ToCreateItemBody()).ToList(),
        };

    public static CreateTodoItemBody ToCreateItemBody(this TodoListItem item) =>
        new()
        {
            Description = item.Name,
            Completed = item.IsComplete,
            SourceId = item.Id.ToSourceId(),
        };

    public static UpdateTodoListBody ToUpdateBody(this TodoList list) => new() { Name = list.Name };

    public static UpdateTodoItemBody ToUpdateBody(this TodoListItem item) =>
        new() { Description = item.Name, Completed = item.IsComplete };

    /// <summary>Create a new local list from an external one (inbound, remote-origin list).</summary>
    public static TodoList ToNewTodoList(this ExternalTodoList external) =>
        new()
        {
            Name = external.Name,
            ExternalId = external.Id,
            CreatedAt = external.CreatedAt,
            UpdatedAt = external.UpdatedAt,
            Items = external.Items.Select(item => item.ToNewTodoListItem()).ToList(),
        };

    public static TodoListItem ToNewTodoListItem(this ExternalTodoItem external) =>
        new()
        {
            Name = external.Description,
            IsComplete = external.Completed,
            ExternalId = external.Id,
            CreatedAt = external.CreatedAt,
            UpdatedAt = external.UpdatedAt,
        };
}
