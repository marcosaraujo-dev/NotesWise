using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NotesWise.API.Endpoints;
using NotesWise.API.Extensions;
using NotesWise.API.Middleware;
using NotesWise.API.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();


// Add services to the container
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddOpenApi();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // More permissive CORS for development
            policy.WithOrigins(
                    "http://localhost:3000",    // Create React App
                    "http://localhost:5173",    // Vite default
                    "http://localhost:8080",    // Alternative Vite port
                    "http://localhost:4173",    // Vite preview
                    "http://[::]:8080",         // IPv6 localhost
                    "http://192.168.1.5:8080"   // Network IP
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .SetIsOriginAllowed(origin => true); // Allow any origin in development
        }
        else
        {
            // Strict CORS for production
            policy.WithOrigins("https://your-production-domain.com")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

// Configure MongoDB
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<MongoDbSettings>>().Value);

builder.Services.AddScoped<IMongoClient>(serviceProvider =>
{
    var settings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>()
        ?? throw new InvalidOperationException("MongoDB settings not configured");
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    var settings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>()
        ?? throw new InvalidOperationException("MongoDB settings not configured");
    return client.GetDatabase(settings.DatabaseName);
});

// AI Services
builder.Services.AddAiServices(builder.Configuration);

// Data Store
builder.Services.AddScoped<IDataStore, MongoDataStore>();


// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

// Use CORS before HTTPS redirection
app.UseCors("AllowReactApp");

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Use authentication and authorization
app.UseAuthentication();
app.UseSupabaseAuth();
app.UseAuthorization();

// Map API endpoints
app.MapCategoryEndpoints();
app.MapNoteEndpoints();
app.MapFlashcardEndpoints();
app.MapAiEndpoints();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi();

// Debug auth endpoint
app.MapGet("/debug/auth", (HttpContext context, IConfiguration config) =>
{
    var userId = context.Items["UserId"]?.ToString();
    var hasAuthHeader = context.Request.Headers.Authorization.Count > 0;
    var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
    
    return Results.Ok(new
    {
        hasAuthHeader,
        authHeaderValue = hasAuthHeader ? authHeader?[..20] + "..." : null,
        userId,
        isAuthenticated = !string.IsNullOrEmpty(userId),
        userClaims = context.User?.Claims?.Select(c => new { c.Type, c.Value }).ToList(),
        supabaseConfig = new
        {
            url = config["Supabase:Url"],
            hasKey = !string.IsNullOrEmpty(config["Supabase:Key"]),
            jwksEndpoint = $"{config["Supabase:Url"]}/auth/v1/jwks"
        }
    });
})
    .WithName("DebugAuth")
    .WithOpenApi();

// Test JWKS endpoint
app.MapGet("/debug/jwks", async (IConfiguration config, HttpClient httpClient) =>
{
    try
    {
        var jwksUrl = $"{config["Supabase:Url"]}/auth/v1/jwks";
        var response = await httpClient.GetAsync(jwksUrl);
        var content = await response.Content.ReadAsStringAsync();
        
        return Results.Ok(new
        {
            jwksUrl,
            status = response.StatusCode,
            content = response.IsSuccessStatusCode ? content : null,
            error = !response.IsSuccessStatusCode ? content : null
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            error = ex.Message,
            type = ex.GetType().Name
        });
    }
})
    .WithName("TestJWKS")
    .WithOpenApi();

app.Run();