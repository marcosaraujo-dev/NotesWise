# NotesWise Backend API

> API robusta e escalável construída com .NET 9 e MongoDB

![.NET](https://img.shields.io/badge/.NET-9.0-512bd4.svg)
![MongoDB](https://img.shields.io/badge/MongoDB-4.4+-47a248.svg)
![C#](https://img.shields.io/badge/C%23-12.0-239120.svg)
![Swagger](https://img.shields.io/badge/OpenAPI-3.0-85ea2d.svg)

## 📋 Visão Geral

O backend do NotesWise é uma Web API moderna construída com .NET 9 que fornece uma base sólida e escalável para o sistema de anotações inteligentes. A API utiliza Minimal APIs para alta performance, MongoDB para persistência de dados e integração com serviços de IA para funcionalidades avançadas.

## 🏗️ Arquitetura

### Arquitetura Limpa (Clean Architecture)

```
NotesWise.API/
├── Endpoints/              # Definição de rotas e endpoints
│   ├── CategoryEndpoints.cs
│   ├── NoteEndpoints.cs
│   ├── FlashcardEndpoints.cs
│   └── AiEndpoints.cs
├── Models/                 # Modelos de dados e DTOs
│   ├── Category.cs
│   ├── Note.cs
│   ├── Flashcard.cs
│   ├── AiModels.cs
│   └── Requests/           # Request/Response DTOs
├── Services/               # Lógica de negócio
│   ├── IDataStore.cs       # Interface do repositório
│   ├── MongoDataStore.cs   # Implementação MongoDB
│   ├── IAiService.cs       # Interface de IA
│   ├── SupabaseAiService.cs # Implementação Supabase
│   └── Models/             # Modelos específicos de serviços
├── Middleware/             # Middleware customizado
│   └── SupabaseAuthMiddleware.cs
├── Extensions/             # Métodos de extensão
│   └── HttpContextExtensions.cs
├── Configuration/          # Configurações
└── Program.cs              # Bootstrap da aplicação
```

### Padrões Arquiteturais

#### 1. **Repository Pattern**
```csharp
public interface IDataStore
{
    Task<IEnumerable<Note>> GetNotesAsync(string userId, string? categoryId = null);
    Task<Note> CreateNoteAsync(Note note);
    Task<Note?> UpdateNoteAsync(Note note);
    Task<bool> DeleteNoteAsync(string id, string userId);
}
```

#### 2. **Dependency Injection**
```csharp
// Program.cs - Configuração de DI
builder.Services.AddScoped<IDataStore, MongoDataStore>();
builder.Services.AddScoped<IAiService, SupabaseAiService>();
builder.Services.AddScoped<IAiProviderFactory, AiProviderFactory>();
```

#### 3. **Minimal APIs Pattern**
```csharp
// Endpoints organizados em classes estáticas
public static class NoteEndpoints
{
    public static void MapNoteEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/notes").WithTags("Notes");
        
        group.MapGet("", GetNotes).WithName("GetNotes");
        group.MapPost("", CreateNote).WithName("CreateNote");
        // ...
    }
}
```

#### 4. **Middleware Pipeline**
```csharp
// Pipeline de middleware configurado
app.UseMiddleware<SupabaseAuthMiddleware>();
app.UseCors("AllowSpecificOrigins");
```

## 🚀 Tecnologias e Dependências

### Core Framework
- **.NET 9**: Framework principal com performance otimizada
- **ASP.NET Core**: Web framework para APIs
- **Minimal APIs**: Abordagem moderna para endpoints

### Banco de Dados
- **MongoDB.Driver**: Driver oficial do MongoDB
- **Connection Pooling**: Pool de conexões para alta performance
- **Indexes**: Otimização de consultas

### Autenticação e Segurança
- **JWT Bearer**: Validação de tokens JWT
- **Supabase Auth**: Integração com autenticação externa
- **CORS**: Configuração de compartilhamento de recursos

### Integrações e IA
- **HttpClient**: Cliente HTTP para APIs externas
- **Supabase Functions**: Integração com funções serverless
- **OpenAI**: Serviços de IA para texto
- **ElevenLabs**: Síntese de voz

### Desenvolvimento e Qualidade
- **Swagger/OpenAPI**: Documentação automática
- **ILogger**: Sistema de logging estruturado
- **Configuration**: Gerenciamento de configurações

## 🗄️ Modelagem de Dados

### Entidades Principais

#### **Note (Nota)**
```csharp
public class Note
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string AudioUrl { get; set; } = string.Empty;
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string? CategoryId { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### **Category (Categoria)**
```csharp
public class Category
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}
```

#### **Flashcard**
```csharp
public class Flashcard
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string NoteId { get; set; } = string.Empty;
    
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    
    public string QuestionAudioUrl { get; set; } = string.Empty;
    public string AnswerAudioUrl { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
}
```

### Relacionamentos e Índices

```csharp
// Índices otimizados para consultas frequentes
notes.Indexes.CreateOne(new CreateIndexModel<Note>(
    Builders<Note>.IndexKeys.Ascending(n => n.UserId)
));

notes.Indexes.CreateOne(new CreateIndexModel<Note>(
    Builders<Note>.IndexKeys
        .Ascending(n => n.UserId)
        .Ascending(n => n.CategoryId)
));

categories.Indexes.CreateOne(new CreateIndexModel<Category>(
    Builders<Category>.IndexKeys.Ascending(c => c.UserId)
));
```

## 🔧 Serviços e Lógica de Negócio

### Data Store Service

```csharp
public class MongoDataStore : IDataStore
{
    private readonly IMongoDatabase _database;
    private readonly IMongoCollection<Note> _notes;
    private readonly IMongoCollection<Category> _categories;
    private readonly IMongoCollection<Flashcard> _flashcards;
    
    public async Task<IEnumerable<Note>> GetNotesAsync(string userId, string? categoryId = null)
    {
        var filterBuilder = Builders<Note>.Filter;
        var filter = filterBuilder.Eq(n => n.UserId, userId);
        
        if (!string.IsNullOrEmpty(categoryId))
        {
            filter = filterBuilder.And(filter, 
                filterBuilder.Eq(n => n.CategoryId, categoryId));
        }
        
        return await _notes
            .Find(filter)
            .SortByDescending(n => n.UpdatedAt)
            .ToListAsync();
    }
}
```

### AI Service Integration

```csharp
public class SupabaseAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    
    public async Task<string> GenerateSummaryAsync(string content, string? provider = null)
    {
        var request = new
        {
            content = content,
            provider = provider ?? "openai"
        };
        
        var response = await _httpClient.PostAsJsonAsync(
            "functions/v1/generate-summary", 
            request
        );
        
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SummaryResponse>();
        
        return result?.Summary ?? string.Empty;
    }
    
    public async Task<string> GenerateAudioAsync(string text, string voice = "burt")
    {
        var request = new { text, voice };
        
        var response = await _httpClient.PostAsJsonAsync(
            "functions/v1/generate-audio", 
            request
        );
        
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<AudioResponse>();
        
        return result?.AudioContent ?? string.Empty;
    }
}
```

## 🔐 Autenticação e Autorização

### JWT Middleware

```csharp
public class SupabaseAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SupabaseAuthMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (ShouldSkipAuth(context.Request.Path))
        {
            await _next(context);
            return;
        }
        
        var token = ExtractBearerToken(context.Request);
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token de autorização necessário");
            return;
        }
        
        try
        {
            var principal = ValidateJwtToken(token);
            var userId = principal.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token inválido: sub claim não encontrado");
                return;
            }
            
            context.Items["UserId"] = userId;
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Falha na autenticação: {Message}", ex.Message);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Token inválido");
        }
    }
}
```

### Extensões de Contexto

```csharp
public static class HttpContextExtensions
{
    public static string GetUserIdOrThrow(this HttpContext context)
    {
        var userId = context.Items["UserId"]?.ToString();
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User ID não encontrado no contexto");
        }
        return userId;
    }
}
```

## 📡 Endpoints da API

### Categories Endpoints

```csharp
// GET /api/categories
private static async Task<IResult> GetCategories(HttpContext context, IDataStore dataStore)
{
    try
    {
        var userId = context.GetUserIdOrThrow();
        var categories = await dataStore.GetCategoriesAsync(userId);
        return Results.Ok(categories);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
}

// POST /api/categories
private static async Task<IResult> CreateCategory(
    HttpContext context, 
    CreateCategoryRequest request, 
    IDataStore dataStore)
{
    try
    {
        var userId = context.GetUserIdOrThrow();
        
        var category = new Category
        {
            Name = request.Name,
            Color = request.Color,
            UserId = userId
        };

        var createdCategory = await dataStore.CreateCategoryAsync(category);
        return Results.Created($"/api/categories/{createdCategory.Id}", createdCategory);
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
}
```

### Notes Endpoints

```csharp
// GET /api/notes?categoryId={categoryId}
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

// POST /api/notes
private static async Task<IResult> CreateNote(
    HttpContext context,
    CreateNoteRequest request,
    IDataStore dataStore,
    IAiService aiService)
{
    try
    {
        var userId = context.GetUserIdOrThrow();

        var note = new Note
        {
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            Summary = request.Summary ?? string.Empty,
            CategoryId = request.CategoryId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Geração automática de resumo se solicitado
        if (request.GenerateSummary && !string.IsNullOrWhiteSpace(request.Content))
        {
            try
            {
                var summary = await aiService.GenerateSummaryAsync(request.Content);
                note.Summary = summary;
            }
            catch (Exception ex)
            {
                // Log erro, mas não falha a criação
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
}
```

### AI Endpoints

```csharp
// POST /api/notes/{id}/generate-summary
private static async Task<IResult> GenerateNoteSummary(
    HttpContext context,
    string id,
    IDataStore dataStore,
    IAiService aiService)
{
    try
    {
        var userId = context.GetUserIdOrThrow();
        var note = await dataStore.GetNoteByIdAsync(id, userId);
        
        if (note == null)
            return Results.NotFound();

        var summary = await aiService.GenerateSummaryAsync(note.Content);
        
        // Atualiza a nota com o resumo gerado
        note.Summary = summary;
        note.UpdatedAt = DateTime.UtcNow;
        await dataStore.UpdateNoteAsync(note);

        return Results.Ok(new GenerateSummaryResponse { Summary = summary });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
}
```

## ⚙️ Configuração e Deploy

### Configurações (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "NotesWise"
  },
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "JwtSecret": "your-jwt-secret-key",
    "ServiceRoleKey": "your-service-role-key"
  },
  "AiProviders": {
    "Default": "openai",
    "OpenAI": {
      "BaseUrl": "https://your-project.supabase.co/functions/v1"
    }
  }
}
```

### Configuração de CORS

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:8080", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddMongoDb(
        mongodbConnectionString: builder.Configuration.GetConnectionString("MongoDB")!,
        name: "mongodb",
        timeout: TimeSpan.FromSeconds(3)
    );

app.MapHealthChecks("/health");
```

## 🧪 Testes e Validação

### Validação de Entrada

```csharp
private static async Task<IResult> CreateNote(
    HttpContext context,
    CreateNoteRequest request,
    IDataStore dataStore,
    IAiService aiService)
{
    // Validações básicas
    if (string.IsNullOrWhiteSpace(request.Title))
        return Results.BadRequest("Title is required");

    if (string.IsNullOrWhiteSpace(request.Content))
        return Results.BadRequest("Content is required");

    // Validação de CategoryId se fornecido
    if (!string.IsNullOrWhiteSpace(request.CategoryId))
    {
        if (!ObjectId.TryParse(request.CategoryId, out _))
            return Results.BadRequest("Invalid CategoryId format");
    }
    
    // ... resto da lógica
}
```

### Testes de Integração

```csharp
[Test]
public async Task CreateNote_ValidRequest_ReturnsCreated()
{
    // Arrange
    var request = new CreateNoteRequest
    {
        Title = "Test Note",
        Content = "Test content",
        GenerateSummary = false
    };

    // Act
    var response = await _client.PostAsJsonAsync("/api/notes", request);

    // Assert
    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
    
    var note = await response.Content.ReadFromJsonAsync<Note>();
    Assert.That(note.Title, Is.EqualTo("Test Note"));
}
```

## 📊 Logging e Monitoramento

### Structured Logging

```csharp
public class MongoDataStore : IDataStore
{
    private readonly ILogger<MongoDataStore> _logger;
    
    public async Task<Note> CreateNoteAsync(Note note)
    {
        _logger.LogInformation("Criando nota {NoteTitle} para usuário {UserId}", 
            note.Title, note.UserId);
        
        try
        {
            await _notes.InsertOneAsync(note);
            
            _logger.LogInformation("Nota criada com sucesso {NoteId}", note.Id);
            return note;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar nota para usuário {UserId}", note.UserId);
            throw;
        }
    }
}
```

### Performance Monitoring

```csharp
// Middleware para monitoramento de performance
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        
        await _next(context);
        
        stopwatch.Stop();
        
        if (stopwatch.ElapsedMilliseconds > 1000) // Log requests > 1s
        {
            _logger.LogWarning("Requisição lenta detectada: {Method} {Path} - {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
```

## 🚀 Performance e Escalabilidade

### Otimizações de Consulta

```csharp
// Paginação eficiente
public async Task<PagedResult<Note>> GetNotesPagedAsync(
    string userId, 
    int page = 1, 
    int pageSize = 20,
    string? categoryId = null)
{
    var filterBuilder = Builders<Note>.Filter;
    var filter = filterBuilder.Eq(n => n.UserId, userId);
    
    if (!string.IsNullOrEmpty(categoryId))
    {
        filter = filterBuilder.And(filter, 
            filterBuilder.Eq(n => n.CategoryId, categoryId));
    }
    
    var totalCount = await _notes.CountDocumentsAsync(filter);
    
    var notes = await _notes
        .Find(filter)
        .SortByDescending(n => n.UpdatedAt)
        .Skip((page - 1) * pageSize)
        .Limit(pageSize)
        .ToListAsync();
    
    return new PagedResult<Note>
    {
        Items = notes,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
}
```

### Caching Strategy

```csharp
// Implementação de cache para categorias
public class CachedDataStore : IDataStore
{
    private readonly IDataStore _innerStore;
    private readonly IMemoryCache _cache;
    
    public async Task<IEnumerable<Category>> GetCategoriesAsync(string userId)
    {
        var cacheKey = $"categories:{userId}";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<Category>? cached))
        {
            return cached!;
        }
        
        var categories = await _innerStore.GetCategoriesAsync(userId);
        
        _cache.Set(cacheKey, categories, TimeSpan.FromMinutes(5));
        
        return categories;
    }
}
```

## 🔄 Roadmap Backend

### Versão Atual (v1.0)
- ✅ API completa com MongoDB
- ✅ Autenticação JWT
- ✅ Integração com IA
- ✅ Documentação OpenAPI

### Próximas Versões
- 🔄 Redis para cache distribuído
- 📊 Métricas e observabilidade
- 🔍 Busca full-text com Elasticsearch
- ⚡ GraphQL endpoint
- 🧪 Cobertura de testes completa
- 🐳 Containerização com Docker

---

<div align="center">
  <p>Backend construído com .NET 9 e as melhores práticas de arquitetura</p>
</div>