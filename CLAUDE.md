# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Frontend (React + Vite + TypeScript)
- `cd frontend && npm run dev` - Start development server on http://localhost:8080
- `cd frontend && npm run build` - Build for production
- `cd frontend && npm run build:dev` - Build in development mode
- `cd frontend && npm run lint` - Run ESLint
- `cd frontend && npm run preview` - Preview production build

### Backend (.NET 9 API)
- `cd backend/NotesWise.API && dotnet run` - Start API server (default: http://localhost:5000)
- `cd backend/NotesWise.API && dotnet build` - Build the API
- `cd backend/NotesWise.API && dotnet restore` - Restore NuGet packages
- `cd backend/NotesWise.API && dotnet test` - Run tests (if any)

### Running the Full Application
1. Start backend: `cd backend/NotesWise.API && dotnet run --urls="http://localhost:5181"`
2. Start frontend: `cd frontend && npm run dev`
3. Access at http://localhost:8080

## Architecture Overview

NotesWise is a hybrid note-taking application with AI features that combines:
- **Frontend**: React with TypeScript, shadcn/ui components, TailwindCSS
- **Backend**: .NET 9 minimal API with MongoDB for data persistence
- **Authentication**: Supabase Auth (frontend) + JWT validation (backend)
- **AI Features**: OpenAI integration via Supabase Functions for summaries, audio, flashcards

### Hybrid Authentication Pattern
- **Frontend**: Uses Supabase Auth for login/register/session management
- **Backend**: Validates Supabase JWT tokens, extracts user_id for data isolation
- **API Client**: Automatically includes JWT tokens in all API requests

### Data Architecture
The application has three main entities:
- **Categories** - User-specific note categories with color coding
- **Notes** - Main content with markdown support, summaries, and audio URLs
- **Flashcards** - Question/answer pairs linked to notes

## Key Files and Components

### Frontend Structure (`frontend/src/`)
- `lib/api-client.ts` - Central API client with all HTTP operations and TypeScript interfaces
- `components/AuthDialog.tsx` - Supabase authentication UI
- `components/NoteEditor.tsx` - Main note editing interface with AI features
- `components/Sidebar.tsx` - Navigation and category management
- `components/FlashcardViewer.tsx` - Flashcard study interface
- `pages/Index.tsx` - Main application page with note listing

### Backend Structure (`backend/NotesWise.API/`)
- `Program.cs` - Application configuration with CORS, MongoDB, and endpoints
- `Endpoints/` - API endpoint definitions (CategoryEndpoints, NoteEndpoints, FlashcardEndpoints, AiEndpoints)
- `Models/` - Data models and DTOs (Category, Note, Flashcard, AI request/response types)
- `Services/` - Business logic (IDataStore, MongoDataStore)
- `Middleware/` - Custom authentication middleware (SupabaseAuthMiddleware)

### Configuration Files
- **Frontend**: `package.json`, `vite.config.ts`, `components.json` (shadcn/ui)
- **Backend**: `NotesWise.API.csproj`, `appsettings.json`, `appsettings.Development.json`

## Development Patterns

### Frontend State Management
- Uses React Query (@tanstack/react-query) for server state management
- Optimistic updates for better UX
- Centralized error handling in API client

### API Integration
- All backend communication goes through `apiClient` singleton
- Automatic JWT token extraction from Supabase session
- Consistent error handling with user-friendly messages
- TypeScript interfaces ensure type safety between frontend/backend

### Component Patterns
- Uses shadcn/ui component library with Radix UI primitives
- TailwindCSS for styling with consistent design system
- Form validation with react-hook-form and zod
- Toast notifications with sonner

### AI Integration
- Supabase Functions handle OpenAI API calls for security
- AI features: text summarization, audio generation, flashcard creation
- Base64 encoded audio content for playback
- Streaming responses where applicable

## MongoDB Configuration

The backend uses MongoDB for data persistence. Configure in `appsettings.json`:
```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "NotesWise"
  }
}
```

## Environment Setup Requirements

### Frontend Dependencies
- Node.js and npm
- All dependencies managed via package.json

### Backend Dependencies  
- .NET 9 SDK
- MongoDB (local or cloud instance)
- Supabase JWT secret for token validation

### Required Environment Variables
- Supabase JWT secret in backend configuration
- MongoDB connection string
- OpenAI API key (configured in Supabase Functions)

## Migration History

This project completed a migration from Supabase database to custom .NET API:
- **Phase 1**: Built .NET API with in-memory storage
- **Phase 2**: Integrated frontend with API client, replaced Supabase data calls
- **Current**: Uses MongoDB for persistence while maintaining Supabase Auth and AI functions