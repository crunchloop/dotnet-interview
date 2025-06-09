using System.Text.Json;

namespace TodoApi.Mcp;

public record InvokeRequest(
    string Tool,
    JsonElement Args
);