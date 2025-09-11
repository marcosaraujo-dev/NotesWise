namespace NotesWise.API.Models
{
    public class AiServiceRequest
    {
        public string Content { get; set; } = string.Empty;
        public string? Model { get; set; }
        public Dictionary<string, object>? Parameters { get; set; }
    }
}
