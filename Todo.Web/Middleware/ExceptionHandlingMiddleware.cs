using Todo.Application.Common.Exceptions;

namespace Todo.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}
