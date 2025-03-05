using DeepSeek.NET;
using DeepSeek.NET.Models;
using Microsoft.Extensions.Configuration;
using System.Text;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();


var apiKey = config["DeepSeek:ApiKey"];

if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.WriteLine("Error: API key not found in appsettings.json");
    Console.WriteLine("Please add your API key under 'DeepSeek:ApiKey'");
    return;
}

Console.WriteLine("DeepSeek API Tester");
Console.WriteLine("-------------------\n");

using var client = new DeepSeekClient(apiKey);

// Select operation mode
Console.WriteLine("\nSelect mode:");
Console.WriteLine("1. Single response");
Console.WriteLine("2. Streaming response");
Console.Write("Your choice (1/2): ");
var mode = Console.ReadLine();

var isStreaming = mode == "2";

// Chat history persistence
var chatHistory = new List<Message>();

while (true)
{
    Console.Write("\nYou: ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

    chatHistory.Add(new Message { Role = RoleType.User, Content = input });

    if (isStreaming)
    {
        await HandleStreamingRequest(client, chatHistory);
    }
    else
    {
        await HandleSingleRequest(client, chatHistory);
    }
}

Console.WriteLine("\nExiting...");

static async Task HandleSingleRequest(DeepSeekClient client, List<Message> chatHistory)
{
    try
    {
        var request = new ChatRequest
        {
            Model = DeepSeekModels.ChatModel,
            Messages = chatHistory,
            Temperature = 0.7f,
            MaxTokens = 1000
        };

        Console.Write("\nAssistant: ");

        var response = await client.ChatAsync(request, new CancellationToken());

        if (response.IsSuccess)
        {
            var responseText = response.Data?.Choices?[0].Message?.Content;

            Console.WriteLine(responseText);

            chatHistory.Add(new Message
            {
                Role = RoleType.Assistant,
                Content = responseText!
            });
        }
        else
        {
            Console.WriteLine($"Error: {response.Error?.Message}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception: {ex.Message}");
    }
}

static async Task HandleStreamingRequest(DeepSeekClient client, List<Message> chatHistory)
{
    try
    {
        var request = new ChatRequest
        {
            Model = DeepSeekModels.ChatModel,
            Messages = chatHistory,
            Temperature = 0.7f,
            MaxTokens = 1000,
            Stream = true
        };

        Console.Write("\nAssistant: ");
        var fullResponse = new StringBuilder();
        await foreach (var chunk in client.ChatStreamAsync(request, new CancellationToken()))
        {
            if (chunk.IsSuccess && chunk.Data?.Choices?.Count > 0)
            {
                var content = chunk.Data.Choices[0].Delta?.Content;
                if (!string.IsNullOrEmpty(content))
                {
                    Console.Write(content);
                    fullResponse.Append(content);
                }
            }
            else if (chunk.Error != null)
            {
                Console.WriteLine($"\nError: {chunk.Error.Message}");
                break;
            }
        }

        chatHistory.Add(new Message
        {
            Role = RoleType.Assistant,
            Content = fullResponse.ToString()
        });

        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nException: {ex.Message}");
    }
}