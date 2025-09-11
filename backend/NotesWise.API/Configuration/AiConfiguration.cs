namespace NotesWise.API.Configuration
{
    public class AiConfiguration
    {
        public string DefaultProvider { get; set; } = "openai";
        public Dictionary<string, AiProviderConfig> Providers { get; set; } = new();
    }
    public class AiProviderConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = string.Empty;
        public Dictionary<string, object> DefaultParameters { get; set; } = new();
        public bool IsEnabled { get; set; } = true;
    }
}
