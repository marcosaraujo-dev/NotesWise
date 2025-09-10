using NotesWise.API.Services.Models;

namespace NotesWise.API.Services
{
    public interface IAiProvider
    {
        string ProviderName { get; }
        Task<AiResponse> GenerateSummaryAsync(AiRequest request);
        Task<AiResponse> GenerateTextAsync(AiRequest request);
        Task<bool> IsHealthyAsync();
    }
}
