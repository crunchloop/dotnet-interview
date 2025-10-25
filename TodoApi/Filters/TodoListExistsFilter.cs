using Microsoft.AspNetCore.Mvc.Filters;
using TodoApi.Application.Interfaces;

public class TodoListExistsFilter : IAsyncActionFilter
{
    private readonly ITodoListService _todoListService;

    public TodoListExistsFilter(ITodoListService todoListService)
    {
        _todoListService = todoListService;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.ActionArguments.TryGetValue("listId", out var listIdObj) && int.TryParse(listIdObj?.ToString(), out var listId))
        {
            var todoListExists = await _todoListService.ListExist(listId);
            if (!todoListExists)
            {
                throw new NotFoundException($"TodoList with ID {listId} not found.");
            }
        }
        else
        {
            throw new BadRequestException("Invalid or missing TodoList ID.");
        }

        await next();
    }
}