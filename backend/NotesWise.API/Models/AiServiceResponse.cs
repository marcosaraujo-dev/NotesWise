namespace NotesWise.API.Models
{
    public class AiServiceResponse
    {
        public string Content { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? Error { get; set; }
        public string? Provider { get; set; }
        public string? Model { get; set; }
    }
}
