using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TodoApi.Mcp;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("mcp")]
    public class McpController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        // Descriptors
        private static readonly List<ToolDescriptor> _toolDescriptors = new()
        {
            new("createTodoItem", "Create an item in a specific list", "/api/todolists/{listId}/items", "POST", new {
                type="object",
                properties=new {
                    listId=new { type="integer" },
                    description=new { type="string" }
                },
                required=new[]{ "listId","description" }
            }),
            new("getTodoItem", "Get an item by its ID in a list", "/api/todolists/{listId}/items/{id}", "GET", new {
                type="object",
                properties=new {
                    listId=new { type="integer" },
                    id=new { type="integer" }
                },
                required=new[]{ "listId","id" }
            }),
            new("updateTodoItem", "Update the description or status of an item", "/api/todolists/{listId}/items/{id}", "PUT", new {
                type="object",
                properties=new {
                    listId=new { type="integer" },
                    id=new { type="integer" },
                    description=new { type="string" },
                    isCompleted=new { type="boolean" }
                },
                required=new[]{ "listId","id","description","isCompleted" }
            }),
            new("completeTodoItem", "Mark an item as completed", "/api/todolists/{listId}/items/{id}/complete", "PATCH", new {
                type="object",
                properties=new {
                    listId=new { type="integer" },
                    id=new { type="integer" }
                },
                required=new[]{ "listId","id" }
            }),
            new("deleteTodoItem", "Removes an item from a list", "/api/todolists/{listId}/items/{id}", "DELETE", new {
                type="object",
                properties=new {
                    listId=new { type="integer" },
                    id=new { type="integer" }
                },
                required=new[]{ "listId","id" }
            })
        };

        public McpController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("tools")]
        public IActionResult GetTools()
        {
            return Ok(_toolDescriptors);
        }

        [HttpPost("invoke")]
        public async Task<IActionResult> InvokeTool([FromBody] InvokeRequest req)
        {
            var descriptor = _toolDescriptors.FirstOrDefault(t => t.Name == req.Tool);
            if (descriptor == null)
                return NotFound(new { error = $"Tool '{req.Tool}' not found." });

            // Builds URL and Body
            string url = descriptor.Url;
            var bodyArgs = new Dictionary<string, object>();
            foreach (var prop in req.Args.EnumerateObject())
            {
                var name = prop.Name;
                if (url.Contains($"{{{name}}}"))
                    url = url.Replace($"{{{name}}}", prop.Value.ToString()!);
                else
                    bodyArgs[name] = JsonDocument.Parse(prop.Value.GetRawText()).RootElement;
            }

            var client = _httpClientFactory.CreateClient("local");
            HttpResponseMessage resp = descriptor.Method switch
            {
                "GET"    => await client.GetAsync(url),
                "DELETE" => await client.DeleteAsync(url),
                _        => await client.SendAsync(new HttpRequestMessage(new HttpMethod(descriptor.Method), url)
                               { Content = JsonContent.Create(bodyArgs) })
            };

            var content     = await resp.Content.ReadAsStringAsync();
            var contentType = resp.Content.Headers.ContentType?.ToString() ?? "application/json";
            var statusCode  = (int)resp.StatusCode;

            return new ContentResult
            {
                Content     = content,
                ContentType = contentType,
                StatusCode  = statusCode
            };
        }
    }
}
