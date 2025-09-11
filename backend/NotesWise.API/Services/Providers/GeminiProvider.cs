using NotesWise.API.Configuration;
using NotesWise.API.Models;
using NotesWise.API.Services.Models;
using System.Text.Json;
using System.Text;

namespace NotesWise.API.Services.Providers
{
    public class GeminiProvider : IAiProvider
    {
        private readonly HttpClient _httpClient;
        private readonly AiProviderConfig _config;
        private readonly ILogger<GeminiProvider> _logger;

        public string ProviderName => "gemini";

        public GeminiProvider(HttpClient httpClient, AiProviderConfig config, ILogger<GeminiProvider> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<AiServiceResponse> GenerateSummaryAsync(AiServiceRequest request)
        {
            try
            {
                var geminiRequest = new
                {
                    contents = new[]
                    {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"Resuma este texto de forma concisa e clara:\n\n{request.Content}" }
                        }
                    }
                },
                    generationConfig = new
                    {
                        temperature = request.Parameters?.GetValueOrDefault("temperature", 0.7),
                        maxOutputTokens = request.Parameters?.GetValueOrDefault("max_tokens", 150),
                        topP = 0.8,
                        topK = 10
                    }
                };

                var json = JsonSerializer.Serialize(geminiRequest);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                // Gemini API URL format: {baseUrl}/v1/models/{model}:generateContent?key={apiKey}
                var model = request.Model ?? _config.DefaultModel;
                var url = $"{_config.BaseUrl}/v1/models/{model}:generateContent?key={_config.ApiKey}";

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = httpContent
                };

                var response = await _httpClient.SendAsync(httpRequest);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, responseText);
                    return new AiServiceResponse
                    {
                        IsSuccess = false,
                        Error = $"API Error: {response.StatusCode}",
                        Provider = ProviderName
                    };
                }

                var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseText);

                // Parse Gemini response structure
                var content = "";
                if (geminiResponse.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var contentObj) &&
                        contentObj.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        var firstPart = parts[0];
                        if (firstPart.TryGetProperty("text", out var textElement))
                        {
                            content = textElement.GetString() ?? "";
                        }
                    }
                }

                if (string.IsNullOrEmpty(content))
                {
                    _logger.LogWarning("Empty content received from Gemini");
                    return new AiServiceResponse
                    {
                        IsSuccess = false,
                        Error = "Empty response from Gemini",
                        Provider = ProviderName
                    };
                }

                return new AiServiceResponse
                {
                    Content = content,
                    IsSuccess = true,
                    Provider = ProviderName,
                    Model = model
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary with Gemini");
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
                var geminiRequest = new
                {
                    contents = new[]
                    {
                    new
                    {
                        parts = new[]
                        {
                            new { text = request.Content }
                        }
                    }
                },
                    generationConfig = new
                    {
                        temperature = request.Parameters?.GetValueOrDefault("temperature", 0.9),
                        maxOutputTokens = request.Parameters?.GetValueOrDefault("max_tokens", 1000),
                        topP = 0.8,
                        topK = 10
                    }
                };

                var json = JsonSerializer.Serialize(geminiRequest);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var model = request.Model ?? _config.DefaultModel;
                var url = $"{_config.BaseUrl}/v1/models/{model}:generateContent?key={_config.ApiKey}";

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = httpContent
                };

                var response = await _httpClient.SendAsync(httpRequest);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, responseText);
                    return new AiServiceResponse
                    {
                        IsSuccess = false,
                        Error = $"API Error: {response.StatusCode}",
                        Provider = ProviderName
                    };
                }

                var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseText);

                var content = "";
                if (geminiResponse.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var contentObj) &&
                        contentObj.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        var firstPart = parts[0];
                        if (firstPart.TryGetProperty("text", out var textElement))
                        {
                            content = textElement.GetString() ?? "";
                        }
                    }
                }

                return new AiServiceResponse
                {
                    Content = content,
                    IsSuccess = !string.IsNullOrEmpty(content),
                    Provider = ProviderName,
                    Model = model
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating text with Gemini");
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
                var geminiRequest = new
                {
                    contents = new[]
                    {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"Crie flashcards de estudo (perguntas e respostas) baseados no seguinte conteúdo:\n\n{request.Content}\n\nRetorne apenas um array JSON válido no formato: [{{\"question\": \"pergunta\", \"answer\": \"resposta\"}}]. Crie entre 5 a 10 flashcards relevantes." }
                        }
                    }
                },
                    generationConfig = new
                    {
                        temperature = 0.3, // Baixa temperatura para resposta mais consistente
                        maxOutputTokens = request.Parameters?.GetValueOrDefault("max_tokens", 1500),
                        topP = 0.8,
                        topK = 10
                    }
                };

                var json = JsonSerializer.Serialize(geminiRequest);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var model = request.Model ?? _config.DefaultModel;
                var url = $"{_config.BaseUrl}/v1/models/{model}:generateContent?key={_config.ApiKey}";

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = httpContent
                };

                var response = await _httpClient.SendAsync(httpRequest);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API error: {StatusCode} - {Content}", response.StatusCode, responseText);
                    return new AiServiceResponse
                    {
                        IsSuccess = false,
                        Error = $"API Error: {response.StatusCode}",
                        Provider = ProviderName
                    };
                }

                var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseText);

                var content = "";
                if (geminiResponse.TryGetProperty("candidates", out var candidates) &&
                    candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var contentObj) &&
                        contentObj.TryGetProperty("parts", out var parts) &&
                        parts.GetArrayLength() > 0)
                    {
                        var firstPart = parts[0];
                        if (firstPart.TryGetProperty("text", out var textElement))
                        {
                            content = textElement.GetString() ?? "";
                        }
                    }
                }

                if (string.IsNullOrEmpty(content))
                {
                    return new AiServiceResponse
                    {
                        IsSuccess = false,
                        Error = "Empty response from Gemini",
                        Provider = ProviderName
                    };
                }

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
                        Model = model
                    };
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error parsing flashcards JSON from Gemini: {Content}", flashcardsText);
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
                _logger.LogError(ex, "Error generating flashcards with Gemini");
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
