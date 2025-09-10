using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NotesWise.API.Services.Models
{
    public class AiRequest
    {
        public string Content { get; set; } = string.Empty;
        public string? Model { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class AiResponse
    {
        public string Content { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? Error { get; set; }
        public string? Provider { get; set; }
        public string? Model { get; set; }
    }

    public class OpenAiRequest
    {
        public string model { get; set; }
        public string input { get; set; }
    }

    public class OpenAiResponse
    {
        public List<OpenAiOutput> output { get; set; }
    }

    public class OpenAiOutput
    {
        public List<OpenAiContent> content { get; set; }
    }

    public class OpenAiContent
    {
        public string text { get; set; }
    }

    public class GeminiTextResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[] Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content Content { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public Part[] Parts { get; set; }
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }
    }

}