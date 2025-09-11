using NotesWise.API.Extensions;
using NotesWise.API.Models;
using NotesWise.API.Services;
using NotesWise.API.Services.Models;

namespace NotesWise.API.Endpoints;

public static class AiEndpoints
{
    public static void MapAiEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ai")
            .WithTags("AI")
            .RequireAuthorization();

        // Endpoint para testar providers
        group.MapPost("/test-summary", TestSummary);

        // Endpoint para verificar saúde dos providers
        group.MapGet("/health", GetProvidersHealth);

        // Endpoint para listar providers disponíveis
        group.MapGet("/providers", GetAvailableProviders);

        // Endpoints para funcionalidades específicas
        group.MapPost("/generate-summary", GenerateSummary);
        group.MapPost("/generate-flashcards", GenerateFlashcards);
        group.MapPost("/generate-audio", GenerateAudio);
    }

    private static async Task<IResult> GenerateSummary(
        HttpContext context,
        GenerateSummaryRequest request,
        IAiService aiService)
    {
        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Content))
                return Results.BadRequest("Content is required");

            var summary = await aiService.GenerateSummaryAsync(request.Content);

            return Results.Ok(new GenerateSummaryResponse { Summary = summary });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to generate summary",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GenerateFlashcards(
        HttpContext context,
        GenerateFlashcardsRequest request,
        IAiService aiService)
    {
        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Content))
                return Results.BadRequest("Content is required");

            var flashcards = await aiService.GenerateFlashcardsAsync(request.Content);

            return Results.Ok(new GenerateFlashcardsResponse { Flashcards = flashcards });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to generate flashcards",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GenerateAudio(
        HttpContext context,
        GenerateAudioRequest request,
        IAiService aiService)
    {
        try
        {
            var userId = context.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Text))
                return Results.BadRequest("Text is required");

            var audioContent = await aiService.GenerateAudioAsync(request.Text, request.Voice);

            return Results.Ok(new GenerateAudioResponse { AudioContent = audioContent });
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Failed to generate audio",
                detail: ex.Message,
                statusCode: 500);
        }
    }


    private static async Task<IResult> TestSummary(
        HttpContext context,
        TestSummaryRequest request,
        IAiService aiService)
    {
        var userId = context.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Content))
            return Results.BadRequest("Content is required");

        try
        {
            var summary = await aiService.GenerateSummaryAsync(request.Content, request.Provider);

            return Results.Ok(new
            {
                Summary = summary,
                Provider = request.Provider ?? "default",
                IsSuccess = !string.IsNullOrEmpty(summary)
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error testing AI service: {ex.Message}");
        }
    }

    private static async Task<IResult> GetProvidersHealth(IAiProviderFactory providerFactory)
    {
        var providers = providerFactory.GetAvailableProviders();
        var healthStatus = new Dictionary<string, bool>();

        foreach (var providerName in providers)
        {
            try
            {
                var provider = providerFactory.CreateProvider(providerName);
                healthStatus[providerName] = await provider.IsHealthyAsync();
            }
            catch
            {
                healthStatus[providerName] = false;
            }
        }

        return Results.Ok(new
        {
            Status = healthStatus.Values.Any(v => v) ? "Healthy" : "Unhealthy",
            Providers = healthStatus
        });
    }

    private static IResult GetAvailableProviders(IAiProviderFactory providerFactory)
    {
        var providers = providerFactory.GetAvailableProviders();
        return Results.Ok(new { Providers = providers });
    }
}
