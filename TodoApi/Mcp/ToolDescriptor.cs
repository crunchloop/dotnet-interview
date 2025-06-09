namespace TodoApi.Mcp;

public record ToolDescriptor(
    string Name,
    string Description,
    string Url,
    string Method,
    object? JsonSchema
);
