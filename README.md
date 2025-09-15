# NotesWise

> Sistema inteligente de anotações com funcionalidades de IA para resumos automáticos, áudio e flashcards

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![React](https://img.shields.io/badge/React-18.x-61dafb.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-512bd4.svg)
![MongoDB](https://img.shields.io/badge/MongoDB-4.4+-47a248.svg)

## 📋 Visão Geral

NotesWise é uma aplicação híbrida de notas que combina a simplicidade do gerenciamento tradicional de anotações com o poder da inteligência artificial. O sistema permite criar, organizar e estudar suas anotações com funcionalidades avançadas como geração automática de resumos, conversão de texto para áudio e criação de flashcards para estudo.

### ✨ Principais Funcionalidades

- **📝 Gerenciamento de Notas**: Criação, edição e organização de anotações com suporte a Markdown
- **🎨 Categorização**: Organize suas notas por categorias com códigos de cores personalizados
- **🤖 IA Integrada**: Resumos automáticos gerados por OpenAI
- **🔊 Síntese de Voz**: Conversão de texto para áudio usando ElevenLabs
- **🃏 Flashcards**: Geração automática de flashcards para estudo baseado no conteúdo das notas
- **🎵 Áudio Interativo**: Reprodução de áudio para notas e flashcards
- **🌙 Temas**: Suporte a tema claro, escuro e automático (sistema)
- **🔐 Autenticação Segura**: Sistema de login/registro com Supabase Auth

## 🏗️ Arquitetura do Sistema

### Arquitetura Híbrida

NotesWise utiliza uma arquitetura híbrida que combina o melhor de diferentes tecnologias:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│                 │    │                 │    │                 │
│   Frontend      │◄──►│   Backend       │◄──►│   Database      │
│   React + TS    │    │   .NET 9 API    │    │   MongoDB       │
│                 │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       
         ▼                       ▼                       
┌─────────────────┐    ┌─────────────────┐              
│                 │    │                 │              
│ Supabase Auth   │    │ OpenAI/ElevenLabs│              
│ (JWT Tokens)    │    │ (AI Functions)   │              
│                 │    │                 │              
└─────────────────┘    └─────────────────┘              
```

#### Componentes Principais:

- **Frontend**: React com TypeScript, TailwindCSS e shadcn/ui
- **Backend**: .NET 9 Minimal API para gerenciamento de dados
- **Autenticação**: Supabase Auth para login/registro + JWT validation no backend
- **Banco de Dados**: MongoDB para persistência de dados
- **IA**: Integração com OpenAI e ElevenLabs via Supabase Functions

## 🚀 Tecnologias Utilizadas

### Frontend
- **React 18** com TypeScript
- **Vite** para build e desenvolvimento
- **TailwindCSS** para estilização
- **shadcn/ui** componentes de interface
- **React Query** para gerenciamento de estado do servidor
- **React Hook Form** para formulários
- **Supabase Client** para autenticação

### Backend
- **.NET 9** Minimal API
- **MongoDB Driver** para persistência
- **JWT Authentication** com Supabase
- **OpenAPI/Swagger** para documentação
- **CORS** configurado para desenvolvimento

### Serviços Externos
- **Supabase**: Autenticação e Functions para IA
- **OpenAI**: Geração de resumos e flashcards
- **ElevenLabs**: Síntese de voz
- **MongoDB**: Banco de dados NoSQL

## 📁 Estrutura do Projeto

```
NotesWise/
├── frontend/                 # Aplicação React
│   ├── src/
│   │   ├── components/      # Componentes reutilizáveis
│   │   ├── pages/           # Páginas da aplicação
│   │   ├── hooks/           # Custom hooks
│   │   ├── lib/             # Utilitários e configurações
│   │   └── integrations/    # Integrações externas
│   ├── public/              # Arquivos estáticos
│   └── README.md            # Documentação do frontend
├── backend/                  # API .NET
│   └── NotesWise.API/
│       ├── Endpoints/       # Definições dos endpoints
│       ├── Models/          # Modelos de dados
│       ├── Services/        # Lógica de negócio
│       ├── Middleware/      # Middleware customizado
│       └── README.md        # Documentação do backend
├── CLAUDE.md                # Instruções para desenvolvimento
└── README.md                # Este arquivo
```

## 🛠️ Configuração e Instalação

### Pré-requisitos

- **Node.js** 18+ e npm
- **.NET 9 SDK**
- **MongoDB** (local ou cloud)
- **Conta Supabase** (para auth e AI functions)

### 1. Clone o Repositório

```bash
git clone <repository-url>
cd NotesWise
```

### 2. Configuração do Frontend

```bash
cd frontend
npm install
cp .env.template .env
# Configure as variáveis de ambiente no .env
npm run dev
```

### 3. Configuração do Backend

```bash
cd backend/NotesWise.API
dotnet restore
# Configure appsettings.json com as credenciais
dotnet run
```

### 4. Variáveis de Ambiente

#### Frontend (.env)
```env
VITE_SUPABASE_URL=your_supabase_url
VITE_SUPABASE_ANON_KEY=your_supabase_anon_key
```

#### Backend (appsettings.json)
```json
{
  "Supabase": {
    "JwtSecret": "your_supabase_jwt_secret"
  },
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "NotesWise"
  }
}
```

## 📊 Modelos de Dados

### Note (Nota)
```json
{
  "id": "string",
  "title": "string",
  "content": "string",
  "summary": "string",
  "audioUrl": "string",
  "categoryId": "string?",
  "userId": "string",
  "createdAt": "datetime",
  "updatedAt": "datetime"
}
```

### Category (Categoria)
```json
{
  "id": "string",
  "name": "string", 
  "color": "string",
  "userId": "string"
}
```

### Flashcard
```json
{
  "id": "string",
  "noteId": "string",
  "question": "string",
  "answer": "string",
  "questionAudioUrl": "string?",
  "answerAudioUrl": "string?",
  "createdAt": "datetime"
}
```

## 🔧 Comandos de Desenvolvimento

### Frontend
```bash
npm run dev          # Servidor de desenvolvimento
npm run build        # Build para produção
npm run lint         # Verificação de código
npm run preview      # Preview da build
```

### Backend
```bash
dotnet run           # Executar API
dotnet build         # Build da API
dotnet test          # Executar testes
```

### Aplicação Completa
```bash
# Terminal 1 - Backend
cd backend/NotesWise.API && dotnet run --urls="http://localhost:5181"

# Terminal 2 - Frontend  
cd frontend && npm run dev
```

## 🔑 Funcionalidades de IA

### Resumos Automáticos
- Geração inteligente de resumos usando GPT
- Preservação do contexto e pontos principais
- Integração transparente no fluxo de criação de notas

### Flashcards Inteligentes
- Criação automática de perguntas e respostas
- Baseado no conteúdo das notas
- Algoritmo de estudo com embaralhamento

### Síntese de Voz
- Conversão de texto para áudio natural
- Múltiplas vozes disponíveis
- Suporte para notas e flashcards

## 🔐 Segurança

- **Autenticação JWT**: Tokens seguros do Supabase
- **Isolamento de Dados**: Cada usuário acessa apenas seus dados
- **Middleware de Autorização**: Validação de tokens em todas as rotas
- **CORS Configurado**: Proteção contra requisições não autorizadas
- **Variáveis de Ambiente**: Credenciais protegidas

## 📈 Roadmap

### Versão Atual (v1.0)
- ✅ Sistema completo de notas
- ✅ Funcionalidades de IA
- ✅ Sistema de temas
- ✅ Autenticação segura

### Próximas Versões
- 🔄 Sincronização offline
- 📱 Aplicativo mobile
- 🤝 Compartilhamento de notas
- 📊 Analytics de estudo
- 🔍 Busca avançada com IA

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📜 Licença

Este projeto está licenciado sob a MIT License - veja o arquivo [LICENSE](LICENSE) para detalhes.

## 📞 Suporte

Para dúvidas ou suporte:

- 📧 Email: [marcos.araso@hotmaill.com]
- 🐛 Issues: [GitHub Issues](link-para-issues)
- 📖 Documentação: Veja os READMEs específicos em `/frontend` e `/backend`

---

<div align="center">
  <p>Ptojeto Desenvolvido para o estudo de uso das APIs de IA.</p>
</div>