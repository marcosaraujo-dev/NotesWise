using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotesWise.API.Services
{
    public interface IAiService
    {
        Task<string> GenerateSummaryAsync(string content, string? providerName = null);
        Task<string> GenerateTextAsync(string prompt, string? providerName = null);
        Task<bool> IsProviderHealthyAsync(string providerName);
    }
}