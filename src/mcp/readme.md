# Dad Joke - Model Context Protocol (MCP) Server

> NOTE: This is under development and not yet ready for use. Contact me for more information!

## Overview

This is a Model Context Protocol (MCP) server implementation built with .NET 10.0. The MCP server provides a communication protocol for facilitating interactions between various components in a model-driven system. This implementation demonstrates how to set up a basic MCP server with custom tools and services.

## MCP Configuration Examples

You can configure this server in different ways depending on where it is running.

### Quick Setup Matrix

| Option | Best for | Pros | Trade-offs |
|---|---|---|---|
| Hosted URL (`sse` + `url`) | Shared environments, team/dev server, cloud-hosted MCP | No local build/startup required; easy to share one endpoint | Requires always-on hosted service and network access |
| Local source (`dotnet run`) | Active development and debugging | Fast edit/run loop; easiest to debug from source | Requires local .NET SDK and project dependencies |
| Docker (`docker run`) | Consistent runtime across machines/CI | Reproducible environment; avoids local runtime drift | Requires Docker installed and images built/pulled |

Rule of thumb:

- Use hosted when you already have a reachable MCP endpoint.
- Use local source when you are changing code frequently.
- Use Docker when you want the same runtime behavior everywhere.

### 1) Already-hosted MCP endpoint (HTTP/SSE)

Use this when the server is already running on a host you can reach by URL.

~~~json
{
    "servers": {
        "dad-jokes-hosted": {
            "type": "sse",
            "url": "https://your-host.example.com/mcp"
        }
    }
}
~~~

If your hosted endpoint is local during development, it may look like this:

~~~json
{
    "servers": {
        "dad-jokes-hosted-local": {
            "type": "sse",
            "url": "http://localhost:3001/mcp"
        }
    }
}
~~~

### 2) Run from local source code (dotnet run)

Use this when you want Copilot to start the project directly from source.

StdIO project:

~~~json
{
    "servers": {
        "dad-jokes-stdio-local": {
            "type": "stdio",
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "C:\\AI\\mcp\\DadJokeMCP\\DadJokeMCPStdIO\\DadJokeMCPStdIO.csproj"
            ]
        }
    }
}
~~~

SSE project (starts local web server):

~~~json
{
    "servers": {
        "dad-jokes-sse-local": {
            "type": "sse",
            "url": "http://localhost:3001/mcp",
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "C:\\AI\\mcp\\DadJokeMCP\\DadJokeMCPSSE\\DadJokeMCPSSE.csproj"
            ]
        }
    }
}
~~~

### 3) Run with Docker

Use these examples after building images from the included Dockerfiles.

Build commands:

~~~powershell
docker build -f DadJokeMCPStdIO/Dockerfile -t dadjokemcp:local .
docker build -f DadJokeMCPSSE/Dockerfile -t dadjokemcp-sse:local .
~~~

StdIO container from mcp.json:

~~~json
{
    "servers": {
        "dad-jokes-stdio-docker": {
            "type": "stdio",
            "command": "docker",
            "args": [
                "run",
                "-i",
                "--rm",
                "dadjokemcp:local"
            ]
        }
    }
}
~~~

SSE container from mcp.json:

~~~json
{
    "servers": {
        "dad-jokes-sse-docker": {
            "type": "sse",
            "url": "http://localhost:3001/mcp",
            "command": "docker",
            "args": [
                "run",
                "--rm",
                "-p",
                "3001:3001",
                "dadjokemcp-sse:local"
            ]
        }
    }
}
~~~

## Features

### Core Components

- **MCP Server**: Built using the ModelContextProtocol library (version 0.1.0-preview.2)
- **Standard I/O Transport**: Uses stdio for communication with clients
- **Custom Tool Integration**: Includes examples of how to create and register MCP tools

### Services

- **DadJokeService**: A sample service that fetches a Dad Joke from an API endpoint
  - Provides methods to retrieve a list of all Dad Jokes or find a specific Dad Joke by name
  - Caches results for better performance

### Available Tools

The server exposes several tools that can be invoked by clients:

#### DadJoke Tools

- **GetDadJoke**: Retrieves a random Dad Joke by name
- **GetDadJokeCategories**: Retrieves a list of Dad Joke categories
- **GetDadJokesByCategory**: Returns a JSON serialized list of all available Dad Jokes for one category

#### Echo Tool

- **Echo**: A simple tool that echoes back the provided message with a "hello" prefix

## Configuration Options

### Hosting Configuration

The server uses Microsoft.Extensions.Hosting (version 9.0.3) which provides:

- Configuration from multiple sources (JSON, environment variables, command line)
- Dependency injection for services
- Logging capabilities

### Logging Options

Several logging providers are available:

- **Console**: Logs to standard output
- **Debug**: Logs for debugging purposes
- **EventLog**: Logs to the system event log (when running on Windows)
- **EventSource**: Provides ETW (Event Tracing for Windows) integration

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Basic understanding of the Model Context Protocol (MCP)
- Docker Desktop (optional, only needed for Docker-based examples)

### Running the Server

1. Clone this repository
2. Navigate to the project directory
3. Build the project: `dotnet build`
4. Configure with VS Code or other client:

~~~json
"dadjokeserver": {
    "type": "stdio",
    "command": "dotnet",
    "args": [
        "run",
        "--project",
        "C:\\AI\\mcp\\DadJokeMCP\\DadJokeMCPStdIO\\DadJokeMCPStdIO.csproj"
    ]
}
~~~

> Update the path to the project

### Extending the Server

To add custom tools:

1. Create a class and mark it with the `[McpServerToolType]` attribute
2. Add methods with the `[McpServerTool]` attribute
3. Optionally add `[Description]` attributes to provide documentation

Example:

```csharp
[McpServerToolType]
public static class CustomTool
{
    [McpServerTool, Description("Description of what the tool does")]
    public static string ToolMethod(string param) => $"Result: {param}";
}
```

## Server-Sent Events Implementation (DadJokeMCPSSE)

### SSE Overview

The `DadJokeMCPSSE` project provides an alternative implementation of the DadJoke MCP server using Server-Sent Events (SSE) over HTTP instead of stdio transport. This implementation runs as a web server, making it ideal for web-based clients and scenarios requiring HTTP-based communication.

> Read more about [SSE best practices for security here](https://modelcontextprotocol.io/docs/concepts/transports#security-warning%3A-dns-rebinding-attacks)

### Features

- **HTTP-based Transport**: Runs on `http://localhost:3001` by default
- **Server-Sent Events**: Enables real-time, one-way communication from server to client
- **ASP.NET Core Integration**: Built using ASP.NET Core's web server capabilities
- **MCP over HTTP**: Implements the Model Context Protocol over HTTP transport

### Running the SSE Server

1. Navigate to the DadJokeMCPSSE directory
2. Build and run the project:
3. Connect and run in VS Code or using MCP Inspector `npx @modelcontextprotocol/inspector`

### Implementation Details

The SSE implementation uses ASP.NET Core's built-in web server capabilities while maintaining the same dadjoke data service and tools as the stdio version. This makes it easy to switch between transport methods while keeping the core functionality intact.

## Project Structure

- **/DadJokeMCPStdIO**: Main stdio project directory
  - **DadJokeService.cs**: Implementation of the service to fetch Dad Joke data
  - **DadJokeTools.cs**: MCP tools for accessing Dad Joke data
  - **Program.cs**: Entry point that configures and starts the MCP server

## Dependencies

- **Microsoft.Extensions.Hosting** (9.0.3): Provides hosting infrastructure
- **ModelContextProtocol** (0.1.0-preview.2): MCP server implementation
- **System.Text.Json** (9.0.3): JSON serialization/deserialization

## License

This project is available under the MIT License.
