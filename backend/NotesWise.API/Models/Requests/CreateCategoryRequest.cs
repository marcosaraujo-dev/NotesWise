namespace NotesWise.API.Models.Requests
{
    // Models/Requests/CategoryRequests.cs - DTOs para endpoints de Categories
    public class CreateCategoryRequest
    {
        public required string Name { get; set; }
        public required string Color { get; set; }
    }

}
