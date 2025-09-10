using NotesWise.API.Configuration;
using NotesWise.API.Services.Models;
using System.Text.Json;
using System.Text;

namespace NotesWise.API.Services.Providers
{
    public class AnthropicProvider : IAiProvider
    {
        private readonly HttpClient _httpClient;
        private readonly AiProviderConfig _config;
        private readonly ILogger<AnthropicProvider> _logger;

        public string ProviderName => "anthropic";

        public AnthropicProvider(HttpClient httpClient, AiProviderConfig config, ILogger<AnthropicProvider> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<AiResponse> GenerateSummaryAsync(AiRequest request)
        {
            try
            {
                var anthropicRequest = new
                {
                    model = request.Model ?? _config.DefaultModel,
                    max_tokens = request.Parameters?.GetValueOrDefault("max_tokens", 150),
                    messages = new[]
                    {
                    new { role = "user", content = $"Resuma este texto de forma concisa: {request.Content}" }
                }
                };

                var json = JsonSerializer.Serialize(anthropicRequest);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_config.BaseUrl}/messages")
                {
                    Content = httpContent
                };
                httpRequest.Headers.Add("x-api-key", _config.ApiKey);
                httpRequest.Headers.Add("anthropic-version", "2023-06-01");

                var response = await _httpClient.SendAsync(httpRequest);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new AiResponse
                    {
                        IsSuccess = false,
                        Error = $"API Error: {response.StatusCode}",
                        Provider = ProviderName
                    };
                }

                var anthropicResponse = JsonSerializer.Deserialize<JsonElement>(responseText);
                var content = anthropicResponse
                    .GetProperty("content")[0]
                    .GetProperty("text")
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
                _logger.LogError(ex, "Error generating summary with Anthropic");
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
