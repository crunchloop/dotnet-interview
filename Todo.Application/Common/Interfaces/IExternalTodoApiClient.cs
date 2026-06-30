using Todo.Application.Integration.ExternalApi.Dtos;

namespace Todo.Application.Common.Interfaces;

/// <summary>
/// Typed client over the external Todo API. The abstraction lives in the application
/// layer; the HTTP implementation (resilience, logging, serialization) lives in
/// infrastructure so the core stays unaware of transport concerns.
/// </summary>
public interface IExternalTodoApiClient
{
    /// <summary>GET /todolists — all remote lists with their items.</summary>
    Task<IReadOnlyList<ExternalTodoList>> GetTodoListsAsync(CancellationToken cancellationToken);

    /// <summary>POST /todolists — create a list (with its items) remotely.</summary>
    Task<ExternalTodoList> CreateTodoListAsync(
        CreateTodoListBody body,
        CancellationToken cancellationToken
    );

    /// <summary>PATCH /todolists/{todoListId} — update a remote list.</summary>
    Task<ExternalTodoList> UpdateTodoListAsync(
        string todoListId,
        UpdateTodoListBody body,
        CancellationToken cancellationToken
    );

    /// <summary>DELETE /todolists/{todoListId} — delete a remote list and its items.</summary>
    Task DeleteTodoListAsync(string todoListId, CancellationToken cancellationToken);

    /// <summary>PATCH /todolists/{todoListId}/todoitems/{todoItemId} — update a remote item.</summary>
    Task<ExternalTodoItem> UpdateTodoItemAsync(
        string todoListId,
        string todoItemId,
        UpdateTodoItemBody body,
        CancellationToken cancellationToken
    );

    /// <summary>DELETE /todolists/{todoListId}/todoitems/{todoItemId} — delete a remote item.</summary>
    Task DeleteTodoItemAsync(
        string todoListId,
        string todoItemId,
        CancellationToken cancellationToken
    );
}
