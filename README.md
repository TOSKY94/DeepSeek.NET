# DeepSeek .NET Client

[![NuGet](https://img.shields.io/nuget/v/DeepSeek.svg)](https://www.nuget.org/packages/DeepSeek.NET.SDK)

[DeepSeek](https://www.deepseek.com) .NET SDK client library. Supports both synchronous and streaming chat completions. 
> **Note:** Please ensure you have your api key or go to [official website](https://platform.deepseek.com/), to register and apply for DeepSeek's ApiKey to be able to use this library.

## Table of Contents
- [Installation](#installation)
- [Getting Started](#getting-started)
- [Features](#features)
- [Models](#models)
- [Error Handling](#error-handling)
- [Configuration](#configuration)
- [Advanced Usage](#advanced-usage)
- [Common Issues](#common-issues)
- [Contributing](#contributing)


## Installation

```bash
dotnet add package DeepSeek.NET.SDK
```

## Getting Started

```csharp
using DeepSeek.NET;
using DeepSeek.NET.Models;

var client = new DeepSeekClient("your-api-key");

var response = await client.ChatAsync(new ChatRequest
{
    Model = DeepSeekModels.ChatModel, // optional
    Messages = new List<Message>
    {
        new Message { Role = RoleType.User, Content = "Hello!" }
    }
});

if (response.IsSuccess)
{
    Console.WriteLine(response.Data?.Choices[0].Message?.Content);
}
```

> [!TIP]
> [usage example](https://github.com/TOSKY94/DeepSeek.NET/tree/main/sample/Sample)

## Features

### 1. Chat Completions
- Synchronous responses
- Streaming support (`IAsyncEnumerable`)
- Temperature control
- Max token limits
- Model validation

### 2. Streaming Responses
```csharp
await foreach (var chunk in client.ChatStreamAsync(request))
{
    Console.Write(chunk.Data?.Choices[0].Delta?.Content);
}
```

### 3. Error Handling
- Network error recovery
- JSON parsing errors
- API error responses
- Timeout detection

### 4. Configuration
- Customizable HTTP client
- Retry policies (extensible)
- Request timeout configuration

## Models

### ChatRequest
| Property       | Type         | Description                          |
|----------------|--------------|--------------------------------------|
| Model          | string       | ID of model to use (required)        |
| Messages       | List<Message>| Conversation history                 |
| Temperature    | float        | Creativity control (0-2, default 0.7)|
| MaxTokens      | int          | Maximum tokens to generate           |
| Stream         | bool         | Enable streaming responses           |

### ChatResponse
| Property       | Type         | Description                          |
|----------------|--------------|--------------------------------------|
| Id             | string       | Unique completion ID                 |
| Choices        | List<Choice> | Generated responses                  |
| Usage          | Usage        | Token usage statistics               |
| Created        | long         | Unix timestamp of creation           |

### Choice (Streaming)
| Property       | Type         | Description                          |
|----------------|--------------|--------------------------------------|
| Delta          | Message      | Incremental content update           |
| FinishReason   | string       | Termination reason                   |
| Index          | int          | Choice position                      |

### Enums
```csharp
public enum RoleType
{
    System,
    User,
    Assistant
}

public enum FinishReason
{
    Stop,
    Length,
    ContentFilter,
    Error
}
```

## Error Handling
Handle errors through the `ServiceResponse<T>` wrapper:

```csharp
if (!response.IsSuccess)
{
    Console.WriteLine($"Error {response.StatusCode}: {response.Error?.Message}");
}
```

**Common Error Types:**
- `400 BadRequest`: Invalid parameters
- `401 Unauthorized`: Invalid API key
- `429 TooManyRequests`: Rate limit exceeded
- `500 ServerError`: DeepSeek API issues

## Configuration

### Retry Policies
Extend the client to implement retry logic:
```csharp
public class ResilientDeepSeekClient : DeepSeekClient
{
    private readonly IRetryPolicy _retryPolicy;
    
    public ResilientDeepSeekClient(string apiKey, IRetryPolicy retryPolicy) 
        : base(apiKey)
    {
        _retryPolicy = retryPolicy;
    }
    
    public override async Task<ServiceResponse<ChatResponse>> ChatAsync(ChatRequest request)
    {
        return await _retryPolicy.ExecuteAsync(() => base.ChatAsync(request));
    }
}
```

## Advanced Usage

### Conversation History
```csharp
var chatHistory = new List<Message>
{
    new Message { Role = RoleType.System, Content = "You are helpful assistant" },
    new Message { Role = RoleType.User, Content = "What's 2+2?" }
};

var response = await client.ChatAsync(new ChatRequest
{
    Model = DeepSeekModels.ChatModel, // optional
    Messages = chatHistory
});

// Add response to history
chatHistory.Add(response.Data?.Choices[0].Message);
```

### Streaming with Cancellation
```csharp
var cts = new CancellationTokenSource();
await foreach (var chunk in client.ChatStreamAsync(request)
    .WithCancellation(cts.Token))
{
    if (userPressedEscape) cts.Cancel();
    Console.Write(chunk.Data?.Choices[0].Delta?.Content);
}
```

## Common Issues

### 1. Empty Responses
- Verify API key permissions
- Check model availability
- Ensure message content is not empty

### 2. Streaming Format Errors
```plaintext
System.Text.Json.JsonException: ':' is an invalid start of a value...
```
- Confirm API endpoint supports streaming
- Verify response format matches SSE specifications

### 3. Timeouts
```csharp
var client = new DeepSeekClient(apiKey)
{
    HttpClient = { Timeout = TimeSpan.FromSeconds(30) }
};
```

## Contributing
1. Fork the repository
2. Create feature branch
3. Submit PR with tests
4. Follow coding conventions

**Repository**: [github.com/TOSKY94/DeepSeek.NET](https://github.com/TOSKY94/DeepSeek.NET)  
**NuGet**: [nuget.org/packages/DeepSeek.NET.SDK](https://www.nuget.org/packages/DeepSeek.NET.SDK)