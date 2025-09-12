using NotesWise.API.Extensions;
using NotesWise.API.Models;
using NotesWise.API.Services;
using MongoDB.Bson;
using NotesWise.API.Services.Models;

namespace NotesWise.API.Endpoints{

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

            // AI-powered endpoints for notes
            group.MapPost("{id}/generate-summary", GenerateNoteSummary)
                .WithName("GenerateNoteSummary")
                .WithOpenApi();

            group.MapPost("{id}/generate-audio", GenerateNoteAudio)
                .WithName("GenerateNoteAudio")
                .WithOpenApi();

            group.MapPost("{id}/flashcards/generate", GenerateNoteFlashcards)
                .WithName("GenerateNoteFlashcards")
                .WithOpenApi();
        }

        private static async Task<IResult> GetNotes(
            HttpContext context,
            IDataStore dataStore,
            string? categoryId = null)
        {
            try
            {
                var userId = context.GetUserIdOrThrow();
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
                var userId = context.GetUserIdOrThrow();

                if (!ObjectId.TryParse(id, out _))
                    return Results.BadRequest("Invalid note ID format");

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
            IAiService aiService) // CORRIGIDO: IAIService -> IAiService
        {
            try
            {
                var userId = context.GetUserIdOrThrow();

                // Validações básicas
                if (string.IsNullOrWhiteSpace(request.Title))
                    return Results.BadRequest("Title is required");

                if (string.IsNullOrWhiteSpace(request.Content))
                    return Results.BadRequest("Content is required");

                // Verificar se CategoryId é válido (se fornecido)
                string? validCategoryId = null;
                if (!string.IsNullOrWhiteSpace(request.CategoryId))
                {
                    if (!ObjectId.TryParse(request.CategoryId, out _))
                        return Results.BadRequest("Invalid CategoryId format");

                    var category = await dataStore.GetCategoryByIdAsync(request.CategoryId, userId);
                    if (category == null)
                        return Results.BadRequest("Category not found");

                    validCategoryId = request.CategoryId;
                }

                var note = new Note
                {
                    Title = request.Title.Trim(),
                    Content = request.Content.Trim(),
                    Summary = request.Summary ?? string.Empty, // Save existing summary if provided
                    AudioUrl = request.AudioUrl ?? string.Empty,
                    CategoryId = validCategoryId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // CORRIGIDO: Só gera resumo se solicitado E não há resumo existente
                if (request.GenerateSummary && !string.IsNullOrWhiteSpace(request.Content) && string.IsNullOrWhiteSpace(request.Summary))
                {
                    try
                    {
                        var summary = await aiService.GenerateSummaryAsync(request.Content);
                        note.Summary = summary;
                    }
                    catch (Exception ex)
                    {
                        // Log do erro, mas não falha a criação da nota
                        Console.WriteLine($"Erro ao gerar resumo: {ex.Message}");
                    }
                }

                var createdNote = await dataStore.CreateNoteAsync(note);

                return Results.Created($"/api/notes/{createdNote.Id}", createdNote);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar nota: {ex.Message}");
                return Results.Problem("Erro interno do servidor");
            }
        }

        private static async Task<IResult> UpdateNote(
            HttpContext context,
            string id,
            UpdateNoteRequest request,
            IDataStore dataStore,
            IAiService aiService) // ADICIONADO: Para gerar resumo se solicitado
        {
            try
            {
                var userId = context.GetUserIdOrThrow();

                if (!ObjectId.TryParse(id, out _))
                    return Results.BadRequest("Invalid note ID format");

                var existingNote = await dataStore.GetNoteByIdAsync(id, userId);
                if (existingNote == null)
                {
                    return Results.NotFound();
                }

                // Verificar se CategoryId é válido (se fornecido)
                if (!string.IsNullOrEmpty(request.CategoryId) && !ObjectId.TryParse(request.CategoryId, out _))
                    return Results.BadRequest("Invalid CategoryId format");

                if (!string.IsNullOrEmpty(request.CategoryId))
                {
                    var category = await dataStore.GetCategoryByIdAsync(request.CategoryId, userId);
                    if (category == null)
                        return Results.BadRequest("Category not found");
                }

                // Update properties
                if (!string.IsNullOrEmpty(request.Title))
                    existingNote.Title = request.Title.Trim();
                if (!string.IsNullOrEmpty(request.Content))
                    existingNote.Content = request.Content.Trim();
                if (request.Summary != null)
                    existingNote.Summary = request.Summary;
                if (request.AudioUrl != null)
                    existingNote.AudioUrl = request.AudioUrl;
                if (request.CategoryId != null)
                    existingNote.CategoryId = string.IsNullOrWhiteSpace(request.CategoryId) ? null : request.CategoryId;

                existingNote.UpdatedAt = DateTime.UtcNow;

                // ADICIONADO: Gerar resumo se solicitado
                if (request.GenerateSummary && !string.IsNullOrWhiteSpace(existingNote.Content))
                {
                    try
                    {
                        var summary = await aiService.GenerateSummaryAsync(existingNote.Content);
                        existingNote.Summary = summary;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao gerar resumo: {ex.Message}");
                    }
                }

                var updatedNote = await dataStore.UpdateNoteAsync(existingNote);
                return updatedNote != null ? Results.Ok(updatedNote) : Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao atualizar nota: {ex.Message}");
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

                if (!ObjectId.TryParse(id, out _))
                    return Results.BadRequest("Invalid note ID format");

                var success = await dataStore.DeleteNoteAsync(id, userId);
                return success ? Results.NoContent() : Results.NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        }

        private static async Task<IResult> GenerateNoteSummary(
            HttpContext context,
            string id,
            IDataStore dataStore,
            IAiService aiService) // CORRIGIDO: IAIService -> IAiService
        {
            try
            {
                var userId = context.GetUserIdOrThrow();

                if (!ObjectId.TryParse(id, out _))
                    return Results.BadRequest("Invalid note ID format");

                var note = await dataStore.GetNoteByIdAsync(id, userId);
                if (note == null)
                {
                    return Results.NotFound();
                }

                if (string.IsNullOrWhiteSpace(note.Content))
                {
                    return Results.BadRequest("Note content is empty");
                }

                var summary = await aiService.GenerateSummaryAsync(note.Content);

                // Update the note with the generated summary
                note.Summary = summary;
                note.UpdatedAt = DateTime.UtcNow;
                await dataStore.UpdateNoteAsync(note);

                return Results.Ok(new GenerateSummaryResponse { Summary = summary });
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Failed to generate summary",
                    detail: ex.Message,
                    statusCode: 500);
            }
        }

        private static async Task<IResult> GenerateNoteAudio(
            HttpContext context,
            string id,
            GenerateAudioRequest request,
            IDataStore dataStore,
            IAiService aiService) // CORRIGIDO: IAIService -> IAiService
        {
            try
            {
                var userId = context.GetUserIdOrThrow();

                if (!ObjectId.TryParse(id, out _))
                    return Results.BadRequest("Invalid note ID format");

                var note = await dataStore.GetNoteByIdAsync(id, userId);
                if (note == null)
                {
                    return Results.NotFound();
                }

                // Use the note content or summary for audio generation
                var textToSpeak = !string.IsNullOrEmpty(note.Summary) ? note.Summary : note.Content;

                if (string.IsNullOrWhiteSpace(textToSpeak))
                {
                    return Results.BadRequest("No content available for audio generation");
                }

                var audioContent = await aiService.GenerateAudioAsync(textToSpeak, request.Voice);

                return Results.Ok(new GenerateAudioResponse { AudioContent = audioContent });
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Failed to generate audio",
                    detail: ex.Message,
                    statusCode: 500);
            }
        }

        private static async Task<IResult> GenerateNoteFlashcards(
            HttpContext context,
            string id,
            IDataStore dataStore,
            IAiService aiService) // CORRIGIDO: IAIService -> IAiService
        {
            try
            {
                var userId = context.GetUserIdOrThrow();

                if (!ObjectId.TryParse(id, out _))
                    return Results.BadRequest("Invalid note ID format");

                var note = await dataStore.GetNoteByIdAsync(id, userId);
                if (note == null)
                {
                    return Results.NotFound();
                }

                if (string.IsNullOrWhiteSpace(note.Content))
                {
                    return Results.BadRequest("Note content is empty");
                }

                var flashcardData = await aiService.GenerateFlashcardsAsync(note.Content);

                // Create flashcard entities and save them
                var flashcards = new List<Flashcard>();

                foreach (var data in flashcardData)
                {
                    var flashcard = new Flashcard
                    {
                        NoteId = id,
                        Question = data.Question,
                        Answer = data.Answer,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createdFlashcard = await dataStore.CreateFlashcardAsync(flashcard);
                    flashcards.Add(createdFlashcard);
                }

                return Results.Ok(flashcards);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Failed to generate flashcards",
                    detail: ex.Message,
                    statusCode: 500);
            }
        }
    }
}