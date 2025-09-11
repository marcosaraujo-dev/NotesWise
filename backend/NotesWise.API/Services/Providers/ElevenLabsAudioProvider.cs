using NotesWise.API.Configuration;
using NotesWise.API.Models;
using NotesWise.API.Services.Models;
using System.Text;
using System.Text.Json;

namespace NotesWise.API.Services.Providers
{
    public class ElevenLabsAudioProvider
    {
        private readonly HttpClient _httpClient;
        private readonly AiProviderConfig _config;
        private readonly ILogger<ElevenLabsAudioProvider> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly Dictionary<string, string> _voiceDictionary;

        public ElevenLabsAudioProvider(HttpClient httpClient, AiProviderConfig config, ILogger<ElevenLabsAudioProvider> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                WriteIndented = false
            };

            _voiceDictionary = new Dictionary<string, string>
        {
            { "burt", "4YYIPFl9wE5c4L2eu2Gb"}
        };
        }

        public async Task<string> GenerateAudioAsync(string text, string voice = "burt")
        {
            try
            {
                var voiceId = _voiceDictionary.GetValueOrDefault(voice, _voiceDictionary["burt"]);

                var request = new ElevenLabsRequest
                {
                    Text = text,
                    Model_Id = "eleven_multilingual_v2",
                    Voice_Settings = new ElevenLabsVoiceSettings
                    {
                        Stability = 0.5,
                        Similarity_Boost = 0.5
                    }
                };

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_config.BaseUrl}/v1/text-to-speech/{voiceId}")
                {
                    Content = httpContent
                };

                httpRequest.Headers.Add("xi-api-key", _config.ApiKey);

                var response = await _httpClient.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("ElevenLabs API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    throw new Exception($"ElevenLabs API error: {response.StatusCode}");
                }

                var audioBytes = await response.Content.ReadAsByteArrayAsync();
                return Convert.ToBase64String(audioBytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating audio with ElevenLabs");
                throw;
            }
        }
    }
}
