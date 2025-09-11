using NotesWise.API.Configuration;
using NotesWise.API.Services;
using NotesWise.API.Services.Providers;

namespace NotesWise.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configuração
            services.Configure<AiConfiguration>(configuration.GetSection("AI"));

            // HttpClient Factory
            services.AddHttpClient();

            // Configurar HttpClients específicos para cada provider
            services.AddHttpClient("openai", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "NotesWise-API/1.0");
            });

            services.AddHttpClient("anthropic", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "NotesWise-API/1.0");
            });
            services.AddHttpClient("gemini", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "NotesWise-API/1.0");
            });

            // Serviços AI
            services.AddScoped<IAiProviderFactory, AiProviderFactory>();
            services.AddScoped<IAiService, AiService>();

            return services;
        }
    }
}
