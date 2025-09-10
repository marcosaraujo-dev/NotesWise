using NotesWise.API.Services.Models;

namespace NotesWise.API.Services
{
    public class AiService : IAiService
    {
        private readonly IAiProviderFactory _providerFactory;
        private readonly ILogger<AiService> _logger;

        public AiService(IAiProviderFactory providerFactory, ILogger<AiService> logger)
        {
            _providerFactory = providerFactory;
            _logger = logger;
        }

        public async Task<string> GenerateSummaryAsync(string content, string? providerName = null)
        {
            try
            {
                var provider = string.IsNullOrEmpty(providerName)
                    ? _providerFactory.CreateDefaultProvider()
                    : _providerFactory.CreateProvider(providerName);

                var request = new AiRequest { Content = content };
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

                var request = new AiRequest { Content = prompt };
                var response = await provider.GenerateTextAsync(request);

                return response.IsSuccess ? response.Content : string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating text with provider {Provider}", providerName ?? "default");
                return string.Empty;
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
    }
}