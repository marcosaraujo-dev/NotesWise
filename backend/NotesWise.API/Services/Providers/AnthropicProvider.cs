using NotesWise.API.Configuration;
using NotesWise.API.Services.Models;
using System.Text.Json;
using System.Text;
using NotesWise.API.Models;

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

        public async Task<AiServiceResponse> GenerateSummaryAsync(AiServiceRequest request)
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
                    _logger.LogError("Anthropic API error: {StatusCode} - {Content}", response.StatusCode, responseText);
                    return new AiServiceResponse
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

                return new AiServiceResponse
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
                return new AiServiceResponse
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Provider = ProviderName
                };
            }
        }

        public async Task<AiServiceResponse> GenerateTextAsync(AiServiceRequest request)
        {
            try
            {
                var anthropicRequest = new
                {
                    model = request.Model ?? _config.DefaultModel,
                    max_tokens = request.Parameters?.GetValueOrDefault("max_tokens", 1000),
                    messages = new[]
                    {
                    new { role = "user", content = request.Content }
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
                    _logger.LogError("Anthropic API error: {StatusCode} - {Content}", response.StatusCode, responseText);
                    return new AiServiceResponse
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

                return new AiServiceResponse
                {
                    Content = content ?? string.Empty,
                    IsSuccess = true,
                    Provider = ProviderName,
                    Model = request.Model ?? _config.DefaultModel
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating text with Anthropic");
                return new AiServiceResponse
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Provider = ProviderName
                };
            }
        }

        public async Task<AiServiceResponse> GenerateFlashcardsAsync(AiServiceRequest request)
        {
            try
            {
                var anthropicRequest = new
                {
                    model = request.Model ?? _config.DefaultModel,
                    max_tokens = request.Parameters?.GetValueOrDefault("max_tokens", 1500),
                    messages = new[]
                    {
                    new {
                        role = "user",
                        content = $"Crie flashcards de estudo (perguntas e respostas) baseados no seguinte conteúdo:\n\n{request.Content}\n\nRetorne apenas um array JSON válido no formato: [{{\"question\": \"pergunta\", \"answer\": \"resposta\"}}]. Crie entre 5 a 10 flashcards relevantes."
                    }
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
                    _logger.LogError("Anthropic API error: {StatusCode} - {Content}", response.StatusCode, responseText);
                    return new AiServiceResponse
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
                    .GetString() ?? "";

                // Processar o JSON dos flashcards
                var flashcardsText = content.Replace("```json", "").Replace("```", "").Trim();

                try
                {
                    var flashcards = JsonSerializer.Deserialize<List<FlashcardData>>(flashcardsText);
                    var serializedFlashcards = JsonSerializer.Serialize(flashcards ?? new List<FlashcardData>());

                    return new AiServiceResponse
                    {
                        Content = serializedFlashcards,
                        IsSuccess = true,
                        Provider = ProviderName,
                        Model = request.Model ?? _config.DefaultModel
                    };
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error parsing flashcards JSON from Anthropic: {Content}", flashcardsText);
                    return new AiServiceResponse
                    {
                        IsSuccess = false,
                        Error = "Failed to parse generated flashcards",
                        Provider = ProviderName
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating flashcards with Anthropic");
                return new AiServiceResponse
                {
                    IsSuccess = false,
                    Error = ex.Message,
                    Provider = ProviderName
                };
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                var testRequest = new AiServiceRequest { Content = "test" };
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
