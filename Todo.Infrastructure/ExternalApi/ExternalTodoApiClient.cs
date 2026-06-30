using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Todo.Application.Common.Interfaces;
using Todo.Application.Integration.ExternalApi.Dtos;

namespace Todo.Infrastructure.ExternalApi;

/// <summary>
/// HTTP implementation of <see cref="IExternalTodoApiClient"/>. Resilience (retry with
/// exponential backoff) is configured on the underlying <see cref="HttpClient"/> in DI;
/// this class focuses on serialization and request/response logging.
/// </summary>
public class ExternalTodoApiClient : IExternalTodoApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly ILogger<ExternalTodoApiClient> _logger;

    public ExternalTodoApiClient(HttpClient http, ILogger<ExternalTodoApiClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ExternalTodoList>> GetTodoListsAsync(
        CancellationToken cancellationToken
    )
    {
        const string path = "todolists";
        _logger.LogDebug("External API request: GET {Path}", path);

        using var response = await _http.GetAsync(path, cancellationToken);
        await EnsureSuccessAsync("GET", path, response, cancellationToken);

        var lists =
            await response.Content.ReadFromJsonAsync<List<ExternalTodoList>>(
                JsonOptions,
                cancellationToken
            ) ?? new List<ExternalTodoList>();

        _logger.LogDebug(
            "External API response: GET {Path} -> {Status}, {Count} list(s).",
            path,
            (int)response.StatusCode,
            lists.Count
        );

        return lists;
    }

    public async Task<ExternalTodoList> CreateTodoListAsync(
        CreateTodoListBody body,
        CancellationToken cancellationToken
    )
    {
        const string path = "todolists";
        _logger.LogDebug(
            "External API request: POST {Path} (source_id={SourceId}, {ItemCount} item(s)).",
            path,
            body.SourceId,
            body.Items.Count
        );

        using var response = await _http.PostAsJsonAsync(
            path,
            body,
            JsonOptions,
            cancellationToken
        );
        await EnsureSuccessAsync("POST", path, response, cancellationToken);

        return await ReadResponseAsync<ExternalTodoList>("POST", path, response, cancellationToken);
    }

    public async Task<ExternalTodoList> UpdateTodoListAsync(
        string todoListId,
        UpdateTodoListBody body,
        CancellationToken cancellationToken
    )
    {
        var path = $"todolists/{todoListId}";
        _logger.LogDebug("External API request: PATCH {Path} (name={Name}).", path, body.Name);

        using var response = await _http.PatchAsJsonAsync(
            path,
            body,
            JsonOptions,
            cancellationToken
        );
        await EnsureSuccessAsync("PATCH", path, response, cancellationToken);

        return await ReadResponseAsync<ExternalTodoList>(
            "PATCH",
            path,
            response,
            cancellationToken
        );
    }

    public async Task DeleteTodoListAsync(string todoListId, CancellationToken cancellationToken)
    {
        var path = $"todolists/{todoListId}";
        _logger.LogDebug("External API request: DELETE {Path}.", path);

        using var response = await _http.DeleteAsync(path, cancellationToken);
        await EnsureSuccessAsync("DELETE", path, response, cancellationToken);

        _logger.LogDebug(
            "External API response: DELETE {Path} -> {Status}.",
            path,
            (int)response.StatusCode
        );
    }

    public async Task<ExternalTodoItem> UpdateTodoItemAsync(
        string todoListId,
        string todoItemId,
        UpdateTodoItemBody body,
        CancellationToken cancellationToken
    )
    {
        var path = $"todolists/{todoListId}/todoitems/{todoItemId}";
        _logger.LogDebug(
            "External API request: PATCH {Path} (completed={Completed}).",
            path,
            body.Completed
        );

        using var response = await _http.PatchAsJsonAsync(
            path,
            body,
            JsonOptions,
            cancellationToken
        );
        await EnsureSuccessAsync("PATCH", path, response, cancellationToken);

        return await ReadResponseAsync<ExternalTodoItem>(
            "PATCH",
            path,
            response,
            cancellationToken
        );
    }

    public async Task DeleteTodoItemAsync(
        string todoListId,
        string todoItemId,
        CancellationToken cancellationToken
    )
    {
        var path = $"todolists/{todoListId}/todoitems/{todoItemId}";
        _logger.LogDebug("External API request: DELETE {Path}.", path);

        using var response = await _http.DeleteAsync(path, cancellationToken);
        await EnsureSuccessAsync("DELETE", path, response, cancellationToken);

        _logger.LogDebug(
            "External API response: DELETE {Path} -> {Status}.",
            path,
            (int)response.StatusCode
        );
    }

    private async Task<T> ReadResponseAsync<T>(
        string method,
        string path,
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        var dto =
            await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException(
                $"External API {method} {path} returned an empty body."
            );

        _logger.LogDebug(
            "External API response: {Method} {Path} -> {Status}.",
            method,
            path,
            (int)response.StatusCode
        );

        return dto;
    }

    /// <summary>
    /// Throws with full context (endpoint, status, error body) when the response is not a
    /// success, logging at <c>Error</c> first. The body summary is truncated to keep logs sane.
    /// </summary>
    private async Task EnsureSuccessAsync(
        string method,
        string path,
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string body;
        try
        {
            body = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            body = "<unreadable>";
        }

        if (body.Length > 500)
        {
            body = body[..500];
        }

        _logger.LogError(
            "External API {Method} {Path} failed with {Status}. Body: {Body}",
            method,
            path,
            (int)response.StatusCode,
            body
        );

        throw new HttpRequestException(
            $"External API {method} {path} failed with status {(int)response.StatusCode}."
        );
    }
}
