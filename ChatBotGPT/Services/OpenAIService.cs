using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ChatBotGPT.Models;


namespace ChatBotGPT.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public OpenAIService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> GetChatResponseAsync(List<ChatMessage> messages)
        {
            var apiKey = _config["OpenAI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var request = new
            {
                model = "gpt-3.5-turbo", // یا gpt-4
                messages = messages.Select(m => new { role = m.Role, content = m.Content }),
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseString);
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString();
        }
    }

}
