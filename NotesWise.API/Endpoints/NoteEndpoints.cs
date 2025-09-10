using NotesWise.API.Extensions;
using NotesWise.API.Models;
using NotesWise.API.Services;

namespace NotesWise.API.Endpoints;

public static class NoteEndpoints
{
    public static void MapNoteEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/notes").WithTags("Notes");

        group.MapGet("", GetNotes)
            .WithName("GetNotes")
            .WithOpenApi();

        group.MapGet("{id}", GetNote)
            .WithName("GetNote")
            .WithOpenApi();

        group.MapPost("", CreateNote)
            .WithName("CreateNote")
            .WithOpenApi();

        group.MapPut("{id}", UpdateNote)
            .WithName("UpdateNote")
            .WithOpenApi();

        group.MapDelete("{id}", DeleteNote)
            .WithName("DeleteNote")
            .WithOpenApi();
    }

    private static async Task<IResult> GetNotes(
        HttpContext context, 
        IDataStore dataStore, 
        string? categoryId = null)
    {
        try
        {
            var userId = new Guid("d2be9c69-c33c-4774-9d52-a4357383c442").ToString(); //context.GetUserIdOrThrow();
            var notes = await dataStore.GetNotesAsync(userId, categoryId);
            return Results.Ok(notes);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> GetNote(
        HttpContext context, 
        string id, 
        IDataStore dataStore)
    {
        try
        {
            var userId = new Guid("d2be9c69-c33c-4774-9d52-a4357383c442").ToString(); //context.GetUserIdOrThrow();
            var note = await dataStore.GetNoteByIdAsync(id, userId);
            return note != null ? Results.Ok(note) : Results.NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> CreateNote(
        HttpContext context, 
        CreateNoteRequest request, 
        IDataStore dataStore,
        IAiService aiService)
    {
        try
        {
            //var userId = context.GetUserIdOrThrow();
            
            // Validate category exists if provided
            if (!string.IsNullOrEmpty(request.CategoryId))
            {
                var category = await dataStore.GetCategoryByIdAsync(request.CategoryId, Guid.NewGuid().ToString());
                if (category == null)
                {
                    return Results.BadRequest("Category not found");
                }
            }

            var note = new Note
            {
                Title = request.Title,
                Content = request.Content,
                Summary = request.Summary,
                AudioUrl = request.AudioUrl,
                CategoryId = request.CategoryId,
                UserId = new Guid("d2be9c69-c33c-4774-9d52-a4357383c442").ToString()
            };

            var summary = await aiService.GenerateSummaryAsync(request.Content);
            note.Summary = summary;

            var createdNote = await dataStore.CreateNoteAsync(note);

            return Results.Created($"/api/notes/{createdNote.Id}", createdNote);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> UpdateNote(
        HttpContext context, 
        string id, 
        UpdateNoteRequest request, 
        IDataStore dataStore)
    {
        try
        {
            var userId = context.GetUserIdOrThrow();
            
            var existingNote = await dataStore.GetNoteByIdAsync(id, userId);
            if (existingNote == null)
            {
                return Results.NotFound();
            }

            // Validate category exists if provided
            if (!string.IsNullOrEmpty(request.CategoryId))
            {
                var category = await dataStore.GetCategoryByIdAsync(request.CategoryId, userId);
                if (category == null)
                {
                    return Results.BadRequest("Category not found");
                }
            }

            // Update properties
            if (!string.IsNullOrEmpty(request.Title))
                existingNote.Title = request.Title;
            if (!string.IsNullOrEmpty(request.Content))
                existingNote.Content = request.Content;
            if (request.Summary != null)
                existingNote.Summary = request.Summary;
            if (request.AudioUrl != null)
                existingNote.AudioUrl = request.AudioUrl;
            if (request.CategoryId != null)
                existingNote.CategoryId = request.CategoryId;

            var updatedNote = await dataStore.UpdateNoteAsync(existingNote);
            return updatedNote != null ? Results.Ok(updatedNote) : Results.NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }

    private static async Task<IResult> DeleteNote(
        HttpContext context, 
        string id, 
        IDataStore dataStore)
    {
        try
        {
            var userId = context.GetUserIdOrThrow();
            
            var success = await dataStore.DeleteNoteAsync(id, userId);
            return success ? Results.NoContent() : Results.NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }
}