
namespace DeepSeek.NET
{
    public class DeepSeekModels
    {
        public const string ChatModel = "deepseek-chat";
        public const string CoderModel = "deepseek-coder";
        public const string ReasonerModel = "deepseek-reasoner";
    }

    public class ResponseFormat
    {
        public const string Text = "text";
        public const string JsonObject = "json_object";
    }

    public class RoleType
    {
        public const string System = "system";
        public const string User = "user";
        public const string Assistant = "assistant";
    }
}
