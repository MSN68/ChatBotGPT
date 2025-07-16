using AiApiWrapper.Api.Models;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ChatBotGPT.Services
{
    public class ChatBotService : IChatBotService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        public ChatBotService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://main.gpt-chatbotru-4-o1.ru");
        }
        public async Task<ChatBotResponseObject?> GenerateRiddleAsync(string riddleAnswer, string model = "chatgpt-4o-latest")
        {
            var question = $"فرض کن تو یک نویسنده داستان کودک و تولیدکننده معما و چالش هستی و می توانی با کلمه ای که به تو داده میشود یک معما تولید کنی. این معما باید کوتاه باشد، از نگاه و زبان اول شخص که همان پاسخ معما هست روایت شود، در معما به هیچ وجه پاسخ معما آورده نشود. برای من یک معما درباره «{riddleAnswer}» ایجاد کن  و البته فقط معما را بنویس. از ایموجی استفاده نکن.";
            return await GenerateAnswerAsync(question, model,false);
        }
        public async Task<string?> GenerateRandomRiddleAsync(string model = "chatgpt-4o-latest")
        {
            var question = $"فرض کن تو یک نویسنده داستان کودک و تولیدکننده معما و چالش هستی و می توانی کلماتی انتخاب کنی که به سختی دیگران میتوانند حدس بزنند. یک کلمه در مورد نام شیئ یا وسیله یا موجود پیشنهاد بده. فقط کلمه را بنویس و از توضیح پرهیز کن. از ایموجی استفاده نکن.";

            var payload = new ChatBotPayload
            {
                Stream = false,
                Model = model,
                Temperature = 1,
                PresencePenalty = 2,
                FrequencyPenalty = 2,
                TopP = 1,
                Messages = new List<Message> { new Message { Role = "user", Content = question } }

            };
            var result = await AnswerAsync(payload);
            return result?.Choices.First()!.Message.Content;
        }
        public async Task<string?> ApplyDiacriticsAsync(string riddle)
        {
            var question = $"فرض کن تو یک متخصص زبان و ادبیات فارسی هستی و بطور خاص متخصص گویش فارسی رسمی کشور ایران هستی.  از تو میخواهم که جمله «{riddle}» را اعراب گذاری کنی بطوری که حرفی از آن کاسته نشود. در اعراب گذاری دقت کن که تلفظ کلمه در عربی ملاک نیست و تلفظ کلمه در فارسی رسمی ملاک است. در هنگام گویش جمله توسط ابزارهای متن به صدا اشتباهی ایجاد نشود. فقط جمله تولید شده را بنویس و از ایموجی استفاده نکن.";
            var model = "chatgpt-4o-latest";
            var result = await GenerateAnswerAsync(question, model,false);
            return result?.Choices.First()!.Message.Content;
        }
        public async Task<ChatBotResponseObject?> GenerateAnswerAsync(string question, string model = "deepseek-ai/DeepSeek-V3", bool flag=false)
        {
            // add condition if first question add "Please answer in English language."
            if(flag)
                question = question + "\n لطفا با زبان فارسی با من صحبت کن و جواب من را با زبان فارسی بده";

            var payload = new ChatBotPayload
            {
                Stream = false,
                Model = model,
                Temperature = 0.5,
                PresencePenalty = 0,
                FrequencyPenalty = 0,
                TopP = 1,
                Messages = new List<Message> { new Message { Role = "user", Content = question } }

            };
            return await AnswerAsync(payload);
        }

        //public async Task<string?> Translate(TranslatorPayload payload)
        //{
        //    var question = $"فرض کن تو یک متخصص ادبیات و زبان هستی. از تو میخواهم بعنوان یک مترجم عمل کنی، از تو میخواهم متن «{payload.FromLanguage}» را از زبان «{payload.ToLanguage}» به زبان «{payload.InputString}» ترجمه کنی. فقط جمله تبدیل شده را بنویس و از استفاده ایموجی خودداری کن.";
        //    var model = "chatgpt-4o-latest";
        //    var result = await GenerateAnswerAsync(question, model);
        //    return result?.Choices.First()!.Message.Content;
        //}
        public async Task<ChatBotResponseObject?> AnswerAsync(ChatBotPayload payload)
        {
            var jsonContent = JsonSerializer.Serialize(payload, _jsonSerializerOptions);

            var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestContent.Headers.ContentLength = Encoding.UTF8.GetByteCount(jsonContent);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/openai/v1/chat/completions")
            {
                Content = requestContent
            };
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:133.0) Gecko/20100101 Firefox/133.0");
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Origin", "moz-extension://a84f4dfa-60e8-47fe-9244-2e5721c30374");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Pragma", "no-cache");
            request.Headers.Add("Cache-Control", "no-cache");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBytes = await response.Content.ReadAsByteArrayAsync();
            var contentEncoding = response.Content.Headers.ContentEncoding;

            string responseContent;

            if (contentEncoding.Contains("gzip"))
            {
                // Decompress the gzip response
                using (var compressedStream = new MemoryStream(responseBytes))
                using (var decompressedStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                using (var reader = new StreamReader(decompressedStream))
                {
                    responseContent = await reader.ReadToEndAsync();
                }
            }
            else
            {
                // Handle other encodings or assume plain text
                responseContent = Encoding.UTF8.GetString(responseBytes);
            }

            return JsonSerializer.Deserialize<ChatBotResponseObject>(responseContent, _jsonSerializerOptions);
        }
        public async Task<object?> CompletionsAsync(object payload)
        {
            var jsonContent = JsonSerializer.Serialize(payload, _jsonSerializerOptions);

            var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestContent.Headers.ContentLength = Encoding.UTF8.GetByteCount(jsonContent);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/openai/v1/chat/completions")
            {
                Content = requestContent
            };
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:133.0) Gecko/20100101 Firefox/133.0");
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Origin", "moz-extension://a84f4dfa-60e8-47fe-9244-2e5721c30374");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Pragma", "no-cache");
            request.Headers.Add("Cache-Control", "no-cache");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBytes = await response.Content.ReadAsByteArrayAsync();
            var contentEncoding = response.Content.Headers.ContentEncoding;

            string responseContent;

            if (contentEncoding.Contains("gzip"))
            {
                // Decompress the gzip response
                using (var compressedStream = new MemoryStream(responseBytes))
                using (var decompressedStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                using (var reader = new StreamReader(decompressedStream))
                {
                    responseContent = await reader.ReadToEndAsync();
                }
            }
            else
            {
                // Handle other encodings or assume plain text
                responseContent = Encoding.UTF8.GetString(responseBytes);
            }

            return JsonSerializer.Deserialize<object>(responseContent, _jsonSerializerOptions);
        }

        public async Task StreamCompletionsAsync(Stream responseBody, object payload)
        {
            var jsonContent = JsonSerializer.Serialize(payload, _jsonSerializerOptions);

            var requestContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            requestContent.Headers.ContentLength = Encoding.UTF8.GetByteCount(jsonContent);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/openai/v1/chat/completions")
            {
                Content = requestContent
            };
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:133.0) Gecko/20100101 Firefox/133.0");
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Origin", "moz-extension://a84f4dfa-60e8-47fe-9244-2e5721c30374");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Pragma", "no-cache");
            request.Headers.Add("Cache-Control", "no-cache");

            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var contentEncoding = response.Content.Headers.ContentEncoding;

            if (contentEncoding.Contains("gzip"))
            {
                // Stream the gzip response directly to the response body
                using (var compressedStream = await response.Content.ReadAsStreamAsync())
                using (var decompressedStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    await decompressedStream.CopyToAsync(responseBody);
                }
            }
            else
            {
                // Stream the response directly to the response body
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                {
                    await responseStream.CopyToAsync(responseBody);
                }
            }
        }

    }

}
