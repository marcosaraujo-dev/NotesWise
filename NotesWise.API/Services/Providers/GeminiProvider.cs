using NotesWise.API.Configuration;
using NotesWise.API.Services.Models;

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

        public Task<AiResponse> GenerateSummaryAsync(AiRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<AiResponse> GenerateTextAsync(AiRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsHealthyAsync()
        {
            throw new NotImplementedException();
        }
    }
}
