namespace NotesWise.API.Models.Requests
{
    public class UpdateNoteRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? Summary { get; set; }
        public string? AudioUrl { get; set; }

        private string? _categoryId;
        public string? CategoryId
        {
            get => _categoryId;
            set => _categoryId = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public bool GenerateSummary { get; set; } = false;
    }

}
