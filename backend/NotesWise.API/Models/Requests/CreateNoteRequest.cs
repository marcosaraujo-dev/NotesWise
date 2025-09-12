namespace NotesWise.API.Models.Requests
{

    public class CreateNoteRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        private string? _categoryId;
        public string? CategoryId
        {
            get => _categoryId;
            set => _categoryId = string.IsNullOrWhiteSpace(value) ? null : value;
        }

        public string? AudioUrl { get; set; }
        public bool GenerateSummary { get; set; } = false;
    }

}
