using NotesWise.API.Models;
using NotesWise.API.Services.Models;

namespace NotesWise.API.Services
{
    public interface IAiProvider
    {
        string ProviderName { get; }
        Task<AiServiceResponse> GenerateSummaryAsync(AiServiceRequest request);
        Task<AiServiceResponse> GenerateTextAsync(AiServiceRequest request);
        Task<AiServiceResponse> GenerateFlashcardsAsync(AiServiceRequest request);
        Task<bool> IsHealthyAsync();
    }
}
