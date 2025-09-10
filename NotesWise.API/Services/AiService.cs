using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NotesWise.API.Services.Models;

namespace NotesWise.API.Services
{
    public class AiService : IAiService
    {
        readonly HttpClient _httpClient;
        readonly string _openAiKey;
        readonly string _geminiApiKey;
        // readonly JsonSerializerOptions _jsonOptions;
        public AiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _openAiKey = configuration["OpenAi:ApiKey"] ?? "";
            _geminiApiKey = configuration["Gemini:ApiKey"] ?? "";
        }

        // public async Task<string> GenerateSummaryAsync(string content) // versão GEMINI
        // {
        //     return "";
        // }

        public async Task<string> GenerateSummaryAsync(string content)
        {
            return "";
        }
    }
}