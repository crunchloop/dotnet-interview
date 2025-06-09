# dotnet-interview / TodoApi

[![Open in Coder](https://dev.crunchloop.io/open-in-coder.svg)](https://dev.crunchloop.io/templates/fly-containers/workspace?param.Git%20Repository=git@github.com:crunchloop/dotnet-interview.git)

This is a simple Todo List API built in .NET 8. This project is currently being used for .NET full-stack candidates.

## Database

The project comes with a devcontainer that provisions a SQL Server database. If you are not going to use the devcontainer, make sure to provision a SQL Server database and
update the connection string.

## Build

To build the application:

`dotnet build`

## Run the API

To run the TodoApi in your local environment:

`dotnet run --project TodoApi`

## Test

To run tests:

`dotnet test`

Check integration tests at: (https://github.com/crunchloop/interview-tests)

## Contact

- Martín Fernández (mfernandez@crunchloop.io)

## About Crunchloop

![crunchloop](https://crunchloop.io/logo-blue.png)

We strongly believe in giving back :rocket:. Let's work together [`Get in touch`](https://crunchloop.io/contact).

## Dotnet-Interview / Joel Pedrosa

This fragment of the repository contains modifications to the Todo App, centered in TodoItems and MCP server support.

REST API for managing task lists (TodoList) and their items (TodoItem).

MCP Server exposing two endpoints:

-GET /mcp/tools: discovers available tools.
-POST /mcp/invoke: dynamically invokes tools by name and arguments.

## Prerequisites

-NET 8 SDK
-Docker
-VSCode with Remote - Containers extension

## Running the application

1-Start the SQL Server container:
    a- docker-compose up -d

2- Build and run the API:
    a- cd TodoApi
    b- dotnet build
    c- dotnet run --urls http://localhost:5083

3-Open Swagger UI in your browser to explore and test the endpoints.

## REST API Endpoints

TodoLists
-GET /api/todolists
-GET /api/todolists/{id}
-POST /api/todolists
-PUT /api/todolists/{id}
-DELETE /api/todolists/{id}

TodoItems
-POST /api/todolists/{listId}/items
-GET /api/todolists/{listId}/items/{id}
-GET /api/todolists/{listId}/items
-PUT /api/todolists/{listId}/items/{id}
-PATCH /api/todolists/{listId}/items/{id}/complete
-DELETE /api/todolists/{listId}/items/{id}

## MCP Server Endpoints

Discover Tools
-GET /mcp/tools
    Returns an array of ToolDescriptor objects:

        name: internal tool name (e.g. createTodoItem).

        description: human-readable description.

        url, method: underlying REST endpoint.

        jsonSchema: expected parameters schema.

Invoke Tool
-POST /mcp/invoke
    {
  "tool": "createTodoItem",
  "args": { "listId": 1, "description": "MCP task" }
    }
    Returns the result (body and status code) of the internal API call.

By exposing these two MCP endpoints, any MCP-compatible client (e.g. Claude Desktop or a custom script) can discover and invoke your TodoList operations using natural language prompts.