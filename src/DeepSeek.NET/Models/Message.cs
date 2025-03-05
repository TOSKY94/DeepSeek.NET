
namespace DeepSeek.NET.Models
{
    /// <summary>
    /// Represents a message in the chat conversation
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The role of the message author (system/user/assistant)
        /// </summary>
        public string Role { get; set; } = RoleType.User;

        /// <summary>
        /// The content of the message
        /// </summary>
        public string Content { get; set; } = string.Empty;
    }
}
