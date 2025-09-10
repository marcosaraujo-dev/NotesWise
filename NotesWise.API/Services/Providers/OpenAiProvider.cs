using NotesWise.API.Configuration;
using NotesWise.API.Services.Models;
using System.Text.Json;
using System.Text;

namespace NotesWise.API.Services.Providers
{
    public class OpenAiProvider : IAiProvider
    {
        private readonly HttpClient _httpClient;
        private readonly AiProviderConfig _config;
        private readonly ILogger<OpenAiProvider> _logger;

        public string ProviderName => "openai";

        public OpenAiProvider(HttpClient httpClient, AiProviderConfig config, ILogger<OpenAiProvider> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<AiResponse> GenerateSummaryAsync(AiRequest request)
        {
            try
            {
                var openAiRequest = new
                {
                    model = request.Model ?? _config.DefaultModel,
                    messages = new[]
                    {
                    new { role = "system", content = "Você é um assistente especializado em resumir textos de forma concisa e clara." },
                    new { role = "user", content = $"Resuma este texto de forma concisa: {request.Content}" }
                },
                    max_tokens = request.Parameters?.GetValueOrDefault("max_tokens", 150),
                    temperature = request.Parameters?.GetValueOrDefault("temperature", 0.7)
                };

                var json = JsonSerializer.Serialize(openAiRequest);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_config.BaseUrl}/chat/completions")
                {
                    Content = httpContent
                };
                httpRequest.Headers.Add("Authorization", $"Bearer {_config.ApiKey}");

                var response = await _httpClient.SendAsync(httpRequest);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API error: {StatusCode} - {Response}", response.StatusCode, responseText);
                    return new AiResponse
                    {
                        IsSuccess = false,
                        Error = $"API Error: {response.StatusCode}",
                        Provider = ProviderName
                    };
                }

                var openAiResponse = JsonSerializer.Deserialize<JsonElement>(responseText);
                var content = openAiResponse
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return new AiResponse
                {
                    Content = content ?? string.Empty,
                    IsSuccess = true,
                    Provider = ProviderName,
                    Model = request.Model ?? _config.DefaultModel
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary with OpenAI");
                return new AiResponse
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Provider = ProviderName
                };
            }
        }

        public async Task<AiResponse> GenerateTextAsync(AiRequest request)
        {
            // Implementação similar para geração de texto geral
            return await GenerateSummaryAsync(request);
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var testRequest = new AiRequest { Content = "test" };
                var response = await GenerateSummaryAsync(testRequest);
                return response.IsSuccess;
            }
            catch
            {
                return false;
            }
        }
    }

}
