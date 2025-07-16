namespace AiApiWrapper.Api.Models
{
    public class ChatBotConfig
    {
    }
    public class ChatBotPayload
    {
        public List<Message> Messages { get; set; }
        public bool Stream { get; set; }
        public string Model { get; set; }
        public double Temperature { get; set; }
        public double PresencePenalty { get; set; }
        public double FrequencyPenalty { get; set; }
        public double TopP { get; set; }
    }
    public class Message
    {
        public string Role { get; set; }
        public string Content { get; set; }
    }
    public class Choice
    {
        public int Index { get; set; }
        public Message Message { get; set; }
        public string FinishReason { get; set; }
    }
    public class Usage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }

    public class ChatBotResponseObject
    {
        public string Id { get; set; }
        public string Object { get; set; }
        public long Created { get; set; }
        public string Model { get; set; }
        public string Provider { get; set; }
        public string SystemFingerprint { get; set; }
        public List<Choice> Choices { get; set; }
        public Usage Usage { get; set; }
    }

}
