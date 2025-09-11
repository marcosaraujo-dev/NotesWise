using NotesWise.API.Configuration;
using NotesWise.API.Services.Models;
using System.Text.Json;
using System.Text;
using NotesWise.API.Models;

namespace NotesWise.API.Services.Providers
{

    public class OpenAiProvider : IAiProvider
    {
        private readonly HttpClient _httpClient;
        private readonly AiProviderConfig _config;
        private readonly ILogger<OpenAiProvider> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public string ProviderName => "openai";

        public OpenAiProvider(HttpClient httpClient, AiProviderConfig config, ILogger<OpenAiProvider> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;

            // Inicializar JsonSerializerOptions
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                WriteIndented = false
            };
        }

        public async Task<AiServiceResponse> GenerateSummaryAsync(AiServiceRequest request)
        {
            try
            {
                var openAiRequest = new OpenAIRequest
                {
                    Model = request.Model ?? _config.DefaultModel,
                    Input = new List<OpenAIMessage>
                {
                    new()
                    {
                        Role = "system",
                        Content = new List<OpenAIContent>
                        {
                            new OpenAIContent
                            {
                                Text = "Você é um assistente especializado em criar resumos de estudo. Crie um resumo claro, conciso e bem estruturado do conteúdo fornecido, destacando os pontos principais e conceitos importantes.",
                                Type = "input_text"
                            }
                        }
                    },
                    new()
                    {
                        Role = "user",
                        Content = new List<OpenAIContent>
                        {
                            new OpenAIContent
                            {
                                Text = $"Por favor, resuma o seguinte conteúdo de estudo:\n\n{request.Content}",
                                Type = "input_text"
                            }
                        }
                    }
                }
                };

                return await ExecuteOpenAiRequestAsync(openAiRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary with OpenAI");
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
                var openAiRequest = new OpenAIRequest
                {
                    Model = request.Model ?? _config.DefaultModel,
                    Input = new List<OpenAIMessage>
                {
                    new()
                    {
                        Role = "user",
                        Content = new List<OpenAIContent>
                        {
                            new OpenAIContent
                            {
                                Text = request.Content,
                                Type = "input_text"
                            }
                        }
                    }
                }
                };

                return await ExecuteOpenAiRequestAsync(openAiRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating text with OpenAI");
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
                var openAiRequest = new OpenAIRequest
                {
                    Model = request.Model ?? _config.DefaultModel,
                    Input = new List<OpenAIMessage>
                {
                    new()
                    {
                        Role = "system",
                        Content = new List<OpenAIContent>
                        {
                            new OpenAIContent
                            {
                                Text = "Você é um assistente especializado em criar flashcards de estudo. Crie flashcards no formato de perguntas e respostas baseados no conteúdo fornecido. Retorne um array JSON válido com objetos contendo \"question\" e \"answer\". Crie entre 5 a 10 flashcards relevantes.",
                                Type = "input_text"
                            }
                        }
                    },
                    new()
                    {
                        Role = "user",
                        Content = new List<OpenAIContent>
                        {
                            new OpenAIContent
                            {
                                Text = $"Crie flashcards de estudo (perguntas e respostas) baseados no seguinte conteúdo:\n\n{request.Content}\n\nRetorne apenas um array JSON válido no formato: [{{\"question\": \"pergunta\", \"answer\": \"resposta\"}}]",
                                Type = "input_text"
                            }
                        }
                    }
                }
                };

                var response = await ExecuteOpenAiRequestAsync(openAiRequest);

                if (response.IsSuccess)
                {
                    // Processar o JSON dos flashcards
                    var flashcardsText = response.Content.Replace("```json", "").Replace("```", "").Trim();

                    try
                    {
                        var flashcards = JsonSerializer.Deserialize<List<FlashcardData>>(flashcardsText, _jsonOptions);
                        response.Content = JsonSerializer.Serialize(flashcards ?? new List<FlashcardData>());
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Error parsing flashcards JSON: {Content}", flashcardsText);
                        return new AiServiceResponse
                        {
                            IsSuccess = false,
                            Error = "Failed to parse generated flashcards",
                            Provider = ProviderName
                        };
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating flashcards with OpenAI");
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

        private async Task<AiServiceResponse> ExecuteOpenAiRequestAsync(OpenAIRequest openAiRequest)
        {
            try
            {
                var json = JsonSerializer.Serialize(openAiRequest, _jsonOptions);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_config.BaseUrl}/responses")
                {
                    Content = httpContent
                };
                httpRequest.Headers.Add("Authorization", $"Bearer {_config.ApiKey}");

                var response = await _httpClient.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    return new AiServiceResponse
                    {
                        IsSuccess = false,
                        Error = $"API Error: {response.StatusCode}",
                        Provider = ProviderName
                    };
                }

                var responseContent = await response.Content.ReadAsStringAsync();

                // Log da resposta para debug
                _logger.LogDebug("OpenAI Response: {Response}", responseContent);

                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent, _jsonOptions);

                var content = openAIResponse?.Output?.FirstOrDefault(c => c.Type == "message")?.Content?.FirstOrDefault()?.Text ?? "";

                if (string.IsNullOrEmpty(content))
                {
                    _logger.LogWarning("Empty content received from OpenAI");
                    return new AiServiceResponse
                    {
                        IsSuccess = false,
                        Error = "Empty response from OpenAI",
                        Provider = ProviderName
                    };
                }

                return new AiServiceResponse
                {
                    Content = content,
                    IsSuccess = true,
                    Provider = ProviderName,
                    Model = openAiRequest.Model
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing OpenAI response");
                return new AiServiceResponse
                {
                    IsSuccess = false,
                    Error = "Failed to parse OpenAI response",
                    Provider = ProviderName
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling OpenAI API");
                return new AiServiceResponse
                {
                    IsSuccess = false,
                    Error = "Network error calling OpenAI API",
                    Provider = ProviderName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling OpenAI API");
                return new AiServiceResponse
                {
                    IsSuccess = false,
                    Error = "Unexpected error occurred",
                    Provider = ProviderName
                };
            }
        }
    }
}
