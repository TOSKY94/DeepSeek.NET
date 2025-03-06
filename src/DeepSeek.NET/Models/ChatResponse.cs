using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DeepSeek.NET.Models
{
    /// <summary>
    /// Response structure for successful chat completion
    /// </summary>
    public class ChatResponse
    {
        /// <summary>
        /// Unique ID for the chat completion
        /// </summary>
        public string Id { get; set; } = default!;

        /// <summary>
        /// List of generated message choices
        /// </summary>
        public List<Choice> Choices { get; set; } = new();

        /// <summary>
        /// Token usage statistics
        /// </summary>
        public Usage? Usage { get; set; }

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public long Created { get; set; }
    }

    /// <summary>
    /// Individual chat completion choice
    /// </summary>
    public class Choice
    {
        /// <summary>
        /// Generated message content
        /// </summary>
        public Message? Message { get; set; }

        /// <summary>
        /// Incremental content updates
        /// </summary>
        [JsonPropertyName("delta")]
        public Message? Delta { get; set; }

        /// <summary>
        /// Reason for completion termination
        /// </summary>
        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }

        /// <summary>
        /// Position in the list of choices
        /// </summary>
        public int Index { get; set; }
    }

    /// <summary>
    /// Detailed token usage statistics for the API request
    /// </summary>
    public class Usage
    {
        /// <summary>
        /// Number of tokens used in the completion output
        /// </summary>
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        /// <summary>
        /// Total number of tokens processed from the prompt
        /// </summary>
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        /// <summary>
        /// Number of tokens served from prompt cache
        /// </summary>
        [JsonPropertyName("prompt_cache_hit_tokens")]
        public int PromptCacheHitTokens { get; set; }

        /// <summary>
        /// Number of tokens not found in prompt cache
        /// </summary>
        [JsonPropertyName("prompt_cache_miss_tokens")]
        public int PromptCacheMissTokens { get; set; }

        /// <summary>
        /// Aggregate sum of all tokens used (prompt + completion)
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }

        /// <summary>
        /// Detailed breakdown of prompt token usage
        /// </summary>
        /// <remarks>
        /// Only available when detailed token reporting is enabled
        /// </remarks>

        [JsonPropertyName("prompt_tokens_details")]
        public CompletionTokensDetails? Details { get; set; }

        /// <summary>
        /// Detailed breakdown of different token types used in processing
        /// </summary>
        public class CompletionTokensDetails
        {
            /// <summary>
            /// Tokens used for reasoning/logic processing
            /// </summary>
            [JsonPropertyName("reasoning_tokens")]
            public int ReasoningTokens { get; set; }

            /// <summary>
            /// Tokens served from cache during processing
            /// </summary>
            [JsonPropertyName("cached_tokens")]
            public int CachedTokens { get; set; }
        }
    }
}

