using Microsoft.Extensions.Options;
using NotesWise.API.Configuration;
using NotesWise.API.Services.Providers;

namespace NotesWise.API.Services
{
    public class AiProviderFactory : IAiProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AiConfiguration _config;
        private readonly ILogger<AiProviderFactory> _logger;

        public AiProviderFactory(IServiceProvider serviceProvider, IOptions<AiConfiguration> config, ILogger<AiProviderFactory> logger)
        {
            _serviceProvider = serviceProvider;
            _config = config.Value;
            _logger = logger;
        }

        public IAiProvider CreateProvider(string providerName)
        {
            var providerConfig = _config.Providers.GetValueOrDefault(providerName.ToLower());
            if (providerConfig == null || !providerConfig.IsEnabled)
            {
                throw new ArgumentException($"Provider '{providerName}' not found or not enabled");
            }

            var httpClient = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(providerName);
            var logger = _serviceProvider.GetRequiredService<ILogger<IAiProvider>>();

            return providerName.ToLower() switch
            {
                "openai" => new OpenAiProvider(httpClient, providerConfig, _serviceProvider.GetRequiredService<ILogger<OpenAiProvider>>()),
                "anthropic" => new AnthropicProvider(httpClient, providerConfig, _serviceProvider.GetRequiredService<ILogger<AnthropicProvider>>()),
                "gemini" => new GeminiProvider(httpClient, providerConfig, _serviceProvider.GetRequiredService<ILogger<GeminiProvider>>()),
                _ => throw new ArgumentException($"Unknown provider: {providerName}")
            };
        }

        public IAiProvider CreateDefaultProvider()
        {
            return CreateProvider(_config.DefaultProvider);
        }

        public IEnumerable<string> GetAvailableProviders()
        {
            return _config.Providers
                .Where(p => p.Value.IsEnabled)
                .Select(p => p.Key);
        }
    }

}
