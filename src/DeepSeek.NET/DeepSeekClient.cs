using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DeepSeek.NET.Models;

namespace DeepSeek.NET
{
    /// <summary>
    /// Client for interacting with the DeepSeek API
    /// </summary>
    public class DeepSeekClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private const string BaseUrl = "https://api.deepseek.com/v1/";

        /// <summary>
        /// Initializes a new instance of the DeepSeekClient
        /// </summary>
        /// <param name="apiKey">Your DeepSeek API key</param>
        /// <param name="baseUrl">Optional base API URL</param>
        public DeepSeekClient(string apiKey, string? baseUrl = null)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl ?? BaseUrl),
                Timeout = TimeSpan.FromSeconds(100)
            };

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DeepSeek.NET/1.0");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Sends a chat request to the DeepSeek API
        /// </summary>
        /// <param name="request">Chat request parameters</param>
        /// <returns>Service response with chat completion result</returns>
        public async Task<ServiceResponse<ChatResponse>> ChatAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var validationError = ValidateModelVersion(request.Model);
            if (validationError != null)
            {
                return new ServiceResponse<ChatResponse>
                {
                    IsSuccess = false,
                    Error = validationError,
                    StatusCode = 400
                };
            }

            try
            {
                using var content = new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8,"application/json");

                using var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var error = JsonSerializer.Deserialize<ErrorResponse>(responseContent, _jsonOptions);
                    return HandleErrorResponse(error, response.StatusCode);
                }

                var result = JsonSerializer.Deserialize<ChatResponse>(responseContent, _jsonOptions);
                return new ServiceResponse<ChatResponse>
                {
                    IsSuccess = true,
                    Data = result!,
                    StatusCode = (int)response.StatusCode
                };
            }
            catch (HttpRequestException ex)
            {
                return HandleException(ex, "Network error occurred");
            }
            catch (JsonException ex)
            {
                return HandleException(ex, "Error parsing API response");
            }
            catch (TaskCanceledException ex)
            {
                return HandleException(ex, "Request timed out");
            }
        }

        /// <summary>
        /// Streams chat responses from the API in real-time
        /// </summary>
        /// <param name="request">Chat request parameters (must have Stream = true)</param>
        public async IAsyncEnumerable<ServiceResponse<ChatResponse>> ChatStreamAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (!request.Stream)
            {
                throw new ArgumentException("Stream must be set to true for streaming requests");
            }

            using var content = new StringContent(JsonSerializer.Serialize(request, _jsonOptions), Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var error = JsonSerializer.Deserialize<ErrorResponse>(errorContent, _jsonOptions);
                yield return HandleErrorResponse(error, response.StatusCode);
                yield break;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                if (line.StartsWith("data: "))
                {
                    var jsonContent = line.Substring(6).Trim();
                    if (jsonContent == "[DONE]") break;

                    ServiceResponse<ChatResponse>? chunkResponse = null;
                    try
                    {
                        var chunk = JsonSerializer.Deserialize<ChatResponse>(jsonContent, _jsonOptions);

                        chunkResponse = new ServiceResponse<ChatResponse>
                        {
                            IsSuccess = true,
                            Data = chunk!,
                            StatusCode = (int)response.StatusCode
                        };
                    }
                    catch (JsonException ex)
                    {
                        chunkResponse = HandleErrorResponse(
                            new ErrorResponse
                            {
                                Message = $"JSON parse error: {ex.Message}",
                                Code = "JSON_PARSE_ERROR"
                            },
                            response.StatusCode);
                    }

                    if (chunkResponse != null)
                    {
                        yield return chunkResponse;
                    }
                }
            }
        
        }
        private ErrorResponse? ValidateModelVersion(string model)
        {
            // Add model version validation logic
            var validModels = new[] { "deepseek-chat", "deepseek-coder" };
            return Array.Exists(validModels, m => m == model)
                ? null
                : new ErrorResponse { Message = $"Invalid model: {model}" };
        }

        private ServiceResponse<ChatResponse> HandleErrorResponse(ErrorResponse? error, System.Net.HttpStatusCode statusCode)
        {
            return new ServiceResponse<ChatResponse>
            {
                IsSuccess = false,
                Error = error ?? new ErrorResponse { Message = "Unknown error occurred" },
                StatusCode = (int)statusCode
            };
        }

        private ServiceResponse<ChatResponse> HandleException(Exception ex, string message)
        {
            return new ServiceResponse<ChatResponse>
            {
                IsSuccess = false,
                Error = new ErrorResponse
                {
                    Message = message,
                    Code = ex.GetType().Name,
                    Type = "ClientException"
                },
                StatusCode = 500
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}