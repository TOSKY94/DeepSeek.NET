using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DeepSeek.NET.Models
{
    /// <summary>
    /// Request structure for chat completion API
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// ID of the model to use (e.g., "deepseek-chat")
        /// </summary>
        public string Model { get; set; } = DeepSeekModels.ChatModel;

        /// <summary>
        /// List of messages in the conversation
        /// </summary>
        public List<Message> Messages { get; set; } = new();

        /// <summary>
        /// Sampling temperature (0-2), higher = more creative
        /// </summary>
        public float Temperature { get; set; } = 0.7f;

        /// <summary>
        /// Maximum number of tokens to generate
        /// </summary>
        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 4096;

        /// <summary>
        /// Whether to stream partial results
        /// </summary>
        public bool Stream { get; set; } = false;
    }
}
