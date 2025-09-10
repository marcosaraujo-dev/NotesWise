using MongoDB.Bson;
using MongoDB.Driver;
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
           
            if (string.IsNullOrWhiteSpace(request.Title))
                return Results.BadRequest("Title is required");

            if (string.IsNullOrWhiteSpace(request.Content))
                return Results.BadRequest("Content is required");

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
                
                Title = request.Title.Trim(),
                Content = request.Content.Trim(),
                Summary = request.Summary,
                AudioUrl = request.AudioUrl,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow,
                UserId = new Guid("d2be9c69-c33c-4774-9d52-a4357383c442").ToString()
            };
            var createdNote = await dataStore.CreateNoteAsync(note);

            if (request.GenerateSummary && !string.IsNullOrWhiteSpace(note.Content))
            {
                try
                {
                    // Agora pode especificar o provider ou usar o padrão
                    var summary = await aiService.GenerateSummaryAsync(
                        note.Content,
                        request.AiProvider); // parâmetro opcional

                    if (!string.IsNullOrEmpty(summary))
                    {
                        createdNote.Summary = summary;
                        await dataStore.UpdateNoteAsync(createdNote);
                    }
                }
                catch (Exception ex)
                {
                    // Log do erro de IA, mas não falha a criação da nota
                    Console.WriteLine($"Erro ao gerar resumo: {ex.Message}");
                }
            }

            return Results.Created($"/api/notes/{createdNote.Id}", createdNote);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (MongoException ex)
        {
            Console.WriteLine($"Erro MongoDB: {ex.Message}");
            return Results.Problem("Erro interno do servidor ao salvar nota");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro geral: {ex.Message}");
            return Results.Problem("Erro interno do servidor");
        }
    }

    private static async Task<IResult> UpdateNote(
        HttpContext context,
        string id,
        UpdateNoteRequest request,
        IDataStore dataStore,
        IAiService aiService) 
    {
        var userId = context.GetUserIdOrThrow();

        
        // Validate category exists if provided
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            var category = await dataStore.GetCategoryByIdAsync(request.CategoryId, userId);
            if (category == null)
            {
                return Results.BadRequest("Category not found");
            }
        }


       
        if (string.IsNullOrWhiteSpace(request.Title))
            return Results.BadRequest("Title is required");

        if (string.IsNullOrWhiteSpace(request.Content))
            return Results.BadRequest("Content is required");
        
        var existingNote = await dataStore.GetNoteByIdAsync(id, userId);
        if (existingNote == null)
        {
            return Results.NotFound();
        }

      

        existingNote.Title = request.Title.Trim();
        existingNote.Content = request.Content.Trim();
        existingNote.CategoryId = request.CategoryId;
        existingNote.UpdatedAt = DateTime.UtcNow;

        try
        {
            var updatedNote = await dataStore.UpdateNoteAsync(existingNote);

            // Gerar resumo se habilitado
            if (request.GenerateSummary && !string.IsNullOrWhiteSpace(updatedNote.Content))
            {
                try
                {
                    var summary = await aiService.GenerateSummaryAsync(
                        updatedNote.Content,
                        request.AiProvider);

                    if (!string.IsNullOrEmpty(summary))
                    {
                        updatedNote.Summary = summary;
                        await dataStore.UpdateNoteAsync(updatedNote);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao gerar resumo: {ex.Message}");
                }
            }

            return Results.Ok(updatedNote);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (MongoException ex)
        {
            Console.WriteLine($"Erro MongoDB: {ex.Message}");
            return Results.Problem("Erro interno do servidor ao atualizar nota");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro geral: {ex.Message}");
            return Results.Problem("Erro interno do servidor");
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