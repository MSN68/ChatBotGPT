using AiApiWrapper.Api.Models;

namespace ChatBotGPT.Services
{
    public interface IChatBotService
    {
        Task<ChatBotResponseObject?> GenerateRiddleAsync(string riddleAnswer, string model);
        Task<string?> GenerateRandomRiddleAsync(string model);
        Task<string?> ApplyDiacriticsAsync(string riddle);
        Task<ChatBotResponseObject?> AnswerAsync(ChatBotPayload payload);
        Task<object> CompletionsAsync(object payload);
        Task StreamCompletionsAsync(Stream responseBody, object payload);
        Task<ChatBotResponseObject?> GenerateAnswerAsync(string question, string model, bool flag);
        //Task<string?> Translate(TranslatorPayload payload);
    }
}