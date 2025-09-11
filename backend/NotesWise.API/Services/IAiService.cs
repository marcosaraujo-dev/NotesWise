using NotesWise.API.Models;
using NotesWise.API.Services.Models;

namespace NotesWise.API.Services
{
    public interface IAiService
    {
        // Funcionalidades de texto/resumo
        Task<string> GenerateSummaryAsync(string content, string? providerName = null);
        Task<string> GenerateTextAsync(string prompt, string? providerName = null);
       

        // Funcionalidades de flashcards
        Task<List<FlashcardData>> GenerateFlashcardsAsync(string content, string? providerName = null);

        // Funcionalidades de áudio
        Task<string> GenerateAudioAsync(string text, string voice = "burt");
        Task<GenerateFlashcardAudioResponse> GenerateFlashcardAudioAsync(Flashcard flashcard, string voice = "burt", string type = "both");

        // Funcionalidades de gerenciamento
        Task<bool> IsProviderHealthyAsync(string providerName);
        Task<IEnumerable<string>> GetAvailableProvidersAsync();
    }
}