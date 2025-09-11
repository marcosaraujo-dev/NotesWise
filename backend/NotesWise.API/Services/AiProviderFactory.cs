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
        private readonly ILoggerFactory _loggerFactory;

        public AiProviderFactory(
            IServiceProvider serviceProvider,
            IOptions<AiConfiguration> config,
            ILogger<AiProviderFactory> logger,
            ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _config = config.Value;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public IAiProvider CreateProvider(string providerName)
        {
            var providerConfig = _config.Providers.GetValueOrDefault(providerName.ToLower());
            if (providerConfig == null || !providerConfig.IsEnabled)
            {
                throw new ArgumentException($"Provider '{providerName}' not found or not enabled");
            }

            var httpClient = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(providerName);

            return providerName.ToLower() switch
            {
                "openai" => new OpenAiProvider(httpClient, providerConfig, _loggerFactory.CreateLogger<OpenAiProvider>()),
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
