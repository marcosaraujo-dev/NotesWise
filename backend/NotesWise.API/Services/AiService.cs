using NotesWise.API.Configuration;
using NotesWise.API.Models;
using NotesWise.API.Services.Models;
using NotesWise.API.Services.Providers;
using System.Text.Json;

namespace NotesWise.API.Services
{
    public class AiService : IAiService
    {
        private readonly IAiProviderFactory _providerFactory;
        private readonly ElevenLabsAudioProvider _audioProvider;
        private readonly ILogger<AiService> _logger;

        public AiService(
            IAiProviderFactory providerFactory,
            ILogger<AiService> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILoggerFactory loggerFactory)
        {
            _providerFactory = providerFactory;
            _logger = logger;

            // Configurar ElevenLabs usando ILoggerFactory
            var elevenLabsConfig = new AiProviderConfig
            {
                ApiKey = configuration["ElevenLabs:ApiKey"] ?? throw new InvalidOperationException("ElevenLabs API key not configured"),
                BaseUrl = "https://api.elevenlabs.io"
            };

            var httpClient = httpClientFactory.CreateClient("elevenlabs");
            _audioProvider = new ElevenLabsAudioProvider(
                httpClient,
                elevenLabsConfig,
                loggerFactory.CreateLogger<ElevenLabsAudioProvider>());
        }

        public async Task<string> GenerateSummaryAsync(string content, string? providerName = null)
        {
            try
            {
                var provider = string.IsNullOrEmpty(providerName)
                    ? _providerFactory.CreateDefaultProvider()
                    : _providerFactory.CreateProvider(providerName);

                var request = new AiServiceRequest { Content = content };
                var response = await provider.GenerateSummaryAsync(request);

                if (response.IsSuccess)
                {
                    _logger.LogInformation("Summary generated successfully using {Provider}", response.Provider);
                    return response.Content;
                }
                else
                {
                    _logger.LogWarning("Failed to generate summary: {Error}", response.Error);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary with provider {Provider}", providerName ?? "default");
                return string.Empty;
            }
        }

        public async Task<string> GenerateTextAsync(string prompt, string? providerName = null)
        {
            try
            {
                var provider = string.IsNullOrEmpty(providerName)
                    ? _providerFactory.CreateDefaultProvider()
                    : _providerFactory.CreateProvider(providerName);

                var request = new AiServiceRequest { Content = prompt };
                var response = await provider.GenerateTextAsync(request);

                return response.IsSuccess ? response.Content : string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating text with provider {Provider}", providerName ?? "default");
                return string.Empty;
            }
        }

        public async Task<List<FlashcardData>> GenerateFlashcardsAsync(string content, string? providerName = null)
        {
            try
            {
                var provider = string.IsNullOrEmpty(providerName)
                    ? _providerFactory.CreateDefaultProvider()
                    : _providerFactory.CreateProvider(providerName);

                var request = new AiServiceRequest { Content = content };
                var response = await provider.GenerateFlashcardsAsync(request);

                if (response.IsSuccess)
                {
                    var flashcards = JsonSerializer.Deserialize<List<FlashcardData>>(response.Content);
                    _logger.LogInformation("Flashcards generated successfully using {Provider}", response.Provider);
                    return flashcards ?? new List<FlashcardData>();
                }
                else
                {
                    _logger.LogWarning("Failed to generate flashcards: {Error}", response.Error);
                    return new List<FlashcardData>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating flashcards with provider {Provider}", providerName ?? "default");
                return new List<FlashcardData>();
            }
        }

        public async Task<string> GenerateAudioAsync(string text, string voice = "burt")
        {
            try
            {
                return await _audioProvider.GenerateAudioAsync(text, voice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audio");
                return string.Empty;
            }
        }

        public async Task<GenerateFlashcardAudioResponse> GenerateFlashcardAudioAsync(Flashcard flashcard, string voice = "burt", string type = "both")
        {
            try
            {
                var response = new GenerateFlashcardAudioResponse();

                if (type == "question" || type == "both")
                {
                    var questionAudio = await GenerateAudioAsync(flashcard.Question, voice);
                    response.QuestionAudioContent = questionAudio;
                }

                if (type == "answer" || type == "both")
                {
                    var answerAudio = await GenerateAudioAsync(flashcard.Answer, voice);
                    response.AnswerAudioContent = answerAudio;
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating flashcard audio for flashcard {FlashcardId}", flashcard.Id);
                throw;
            }
        }

        public async Task<bool> IsProviderHealthyAsync(string providerName)
        {
            try
            {
                var provider = _providerFactory.CreateProvider(providerName);
                return await provider.IsHealthyAsync();
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetAvailableProvidersAsync()
        {
            return await Task.FromResult(_providerFactory.GetAvailableProviders());
        }
    }
}
