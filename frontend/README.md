# NotesWise Frontend

> Interface moderna e intuitiva construída com React, TypeScript e TailwindCSS

![React](https://img.shields.io/badge/React-18.x-61dafb.svg)
![TypeScript](https://img.shields.io/badge/TypeScript-5.x-3178c6.svg)
![Vite](https://img.shields.io/badge/Vite-5.x-646cff.svg)
![TailwindCSS](https://img.shields.io/badge/TailwindCSS-3.x-06b6d4.svg)

## 📋 Visão Geral

O frontend do NotesWise é uma Single Page Application (SPA) construída com tecnologias modernas que oferece uma experiência de usuário fluida e responsiva. A aplicação combina um design elegante com funcionalidades avançadas de IA para criar o ambiente perfeito para gerenciar e estudar suas anotações.

## 🏗️ Arquitetura e Padrões

### Arquitetura de Componentes

```
src/
├── components/          # Componentes reutilizáveis
│   ├── ui/             # Componentes base (shadcn/ui)
│   ├── AuthDialog.tsx  # Diálogo de autenticação
│   ├── NoteEditor.tsx  # Editor principal de notas
│   ├── Sidebar.tsx     # Barra lateral de navegação
│   ├── FlashcardViewer.tsx # Visualizador de flashcards
│   └── theme-toggle.tsx # Seletor de tema
├── pages/              # Páginas da aplicação
│   ├── Index.tsx       # Página principal
│   └── NotFound.tsx    # Página 404
├── hooks/              # Custom hooks
│   └── use-theme.tsx   # Hook para gerenciamento de tema
├── lib/                # Utilitários e configurações
│   ├── api-client.ts   # Cliente HTTP para API
│   ├── utils.ts        # Funções utilitárias
│   └── cn.ts          # Utility para classes CSS
├── integrations/       # Integrações externas
│   └── supabase/       # Configuração do Supabase
└── styles/
    └── index.css       # Estilos globais e variáveis CSS
```

### Padrões de Design Implementados

#### 1. **Component Composition Pattern**
```typescript
// Exemplo: NoteEditor composto por múltiplos componentes menores
<NoteEditor>
  <NoteHeader />
  <NoteContent />
  <NoteActions />
</NoteEditor>
```

#### 2. **Custom Hooks Pattern**
```typescript
// Hook personalizado para gerenciamento de tema
const { theme, setTheme } = useTheme();
```

#### 3. **Provider Pattern**
```typescript
// Context para tema global
<ThemeProvider defaultTheme="light">
  <App />
</ThemeProvider>
```

#### 4. **API Client Pattern**
```typescript
// Cliente centralizado para todas as chamadas HTTP
const apiClient = {
  getNotes: (categoryId?: string) => Promise<Note[]>,
  createNote: (note: CreateNoteRequest) => Promise<Note>,
  // ...
};
```

## 🚀 Tecnologias e Ferramentas

### Core Technologies
- **React 18**: Framework principal com Concurrent Features
- **TypeScript**: Tipagem estática para maior segurança
- **Vite**: Build tool moderna e rápida
- **React Router**: Roteamento client-side

### UI/UX
- **TailwindCSS**: Framework CSS utility-first
- **shadcn/ui**: Biblioteca de componentes baseada em Radix UI
- **Radix UI**: Primitivos acessíveis para componentes
- **Lucide React**: Ícones modernos e consistentes

### Estado e Dados
- **TanStack Query (React Query)**: Gerenciamento de estado do servidor
- **React Hook Form**: Gerenciamento de formulários
- **Zustand**: Estado local leve (se necessário)

### Autenticação e API
- **Supabase Client**: Autenticação e sessão
- **Axios/Fetch**: Cliente HTTP customizado
- **JWT**: Tokens para autorização

### Desenvolvimento
- **ESLint**: Linting de código
- **Prettier**: Formatação de código
- **TypeScript Compiler**: Verificação de tipos

## 🎨 Sistema de Design

### Temas e Cores

O NotesWise implementa um sistema de temas robusto com suporte a:

#### **Tema Claro**
```css
:root {
  --primary: 214 57% 42%;        /* #2e5aa8 - Azul principal */
  --background: 0 0% 100%;       /* Branco */
  --foreground: 222.2 84% 4.9%;  /* Texto escuro */
  --card: 0 0% 100%;             /* Cartões brancos */
  --border: 214.3 31.8% 91.4%;  /* Bordas sutis */
}
```

#### **Tema Escuro**
```css
.dark {
  --primary: 214 57% 42%;        /* Mantém azul consistente */
  --background: 222.2 84% 4.9%;  /* Fundo escuro */
  --foreground: 210 40% 98%;     /* Texto claro */
  --card: 222.2 84% 4.9%;        /* Cartões escuros */
  --border: 217.2 32.6% 17.5%;  /* Bordas escuras */
}
```

#### **Tema Sistema**
Detecta automaticamente a preferência do sistema operacional.

### Componentes Base

#### **Buttons**
```typescript
<Button variant="default | destructive | outline | secondary | ghost">
  Texto do Botão
</Button>
```

#### **Cards**
```typescript
<Card>
  <CardHeader>
    <CardTitle>Título</CardTitle>
  </CardHeader>
  <CardContent>
    Conteúdo do cartão
  </CardContent>
</Card>
```

#### **Forms**
```typescript
<Form>
  <FormField
    control={form.control}
    name="title"
    render={({ field }) => (
      <FormItem>
        <FormLabel>Título</FormLabel>
        <FormControl>
          <Input {...field} />
        </FormControl>
      </FormItem>
    )}
  />
</Form>
```

## 📱 Componentes Principais

### 1. **NoteEditor** (`src/components/NoteEditor.tsx`)

Componente principal para criação e edição de notas.

**Funcionalidades:**
- Editor de texto com suporte a Markdown
- Geração automática de resumos
- Conversão texto-para-áudio
- Geração de flashcards
- Auto-save com debounce

**Props:**
```typescript
interface NoteEditorProps {
  note: Note;
  categories: Category[];
  onSave: (note?: Note) => Promise<void>;
  onClose: () => void;
}
```

**Estado:**
```typescript
const [title, setTitle] = useState(note.title);
const [content, setContent] = useState(note.content);
const [summary, setSummary] = useState(note.summary);
const [selectedCategory, setSelectedCategory] = useState(note.categoryId);
const [isGenerating, setIsGenerating] = useState(false);
```

### 2. **Sidebar** (`src/components/Sidebar.tsx`)

Barra lateral para navegação e organização.

**Funcionalidades:**
- Lista de notas filtráveis
- Categorias com cores
- Lista de flashcards
- Busca e filtros

**Seções:**
- **Categories**: Filtro por categoria com indicadores visuais
- **Notes**: Lista de notas com preview do conteúdo
- **Flashcards**: Lista de flashcards para estudo

### 3. **FlashcardViewer** (`src/components/FlashcardViewer.tsx`)

Interface para estudo com flashcards.

**Funcionalidades:**
- Navegação entre flashcards
- Modo de estudo (pergunta → resposta)
- Embaralhamento automático
- Áudio para perguntas e respostas
- Filtros por nota

**Estado do Estudo:**
```typescript
const [currentCardIndex, setCurrentCardIndex] = useState(0);
const [showAnswer, setShowAnswer] = useState(false);
const [shuffledCards, setShuffledCards] = useState<Flashcard[]>([]);
```

### 4. **AuthDialog** (`src/components/AuthDialog.tsx`)

Sistema de autenticação integrado.

**Funcionalidades:**
- Login e registro
- Integração com Supabase Auth
- Validação de formulários
- Estados de loading

## 🔧 API Client

### Estrutura do Cliente HTTP

```typescript
class ApiClient {
  private baseURL: string;
  private getAuthHeaders(): Promise<HeadersInit>;
  
  // Métodos para Notes
  async getNotes(categoryId?: string): Promise<Note[]>;
  async createNote(request: CreateNoteRequest): Promise<Note>;
  async updateNote(id: string, request: UpdateNoteRequest): Promise<Note>;
  async deleteNote(id: string): Promise<void>;
  
  // Métodos para Categories
  async getCategories(): Promise<Category[]>;
  async createCategory(request: CreateCategoryRequest): Promise<Category>;
  
  // Métodos para Flashcards
  async getFlashcards(): Promise<Flashcard[]>;
  async generateFlashcards(noteId: string): Promise<Flashcard[]>;
  
  // Métodos para IA
  async generateSummary(noteId: string): Promise<string>;
  async generateAudio(noteId: string, voice?: string): Promise<string>;
}
```

### Interceptors e Middleware

```typescript
// Interceptor para adicionar JWT automaticamente
const getAuthHeaders = async (): Promise<HeadersInit> => {
  const { data: { session } } = await supabase.auth.getSession();
  
  return {
    'Content-Type': 'application/json',
    ...(session?.access_token && {
      'Authorization': `Bearer ${session.access_token}`
    })
  };
};
```

### Error Handling

```typescript
const handleApiError = (error: unknown): never => {
  if (error instanceof Response) {
    throw new Error(`API Error: ${error.status} ${error.statusText}`);
  }
  throw new Error(error instanceof Error ? error.message : 'Unknown error');
};
```

## 🎯 Gerenciamento de Estado

### Estado Local vs Servidor

#### **Estado do Servidor (React Query)**
```typescript
// Queries para dados do servidor
const { data: notes, isLoading } = useQuery({
  queryKey: ['notes', selectedCategory],
  queryFn: () => apiClient.getNotes(selectedCategory)
});

// Mutations para atualizações
const createNoteMutation = useMutation({
  mutationFn: apiClient.createNote,
  onSuccess: () => {
    queryClient.invalidateQueries(['notes']);
  }
});
```

#### **Estado Local (useState/useReducer)**
```typescript
// Estado da UI e interações
const [selectedNote, setSelectedNote] = useState<Note | null>(null);
const [currentView, setCurrentView] = useState<'notes' | 'flashcards'>('notes');
const [showAuthDialog, setShowAuthDialog] = useState(false);
```

### Context Providers

#### **ThemeProvider**
```typescript
const ThemeProvider = ({ children, defaultTheme = "light" }) => {
  const [theme, setTheme] = useState<Theme>(defaultTheme);
  
  useEffect(() => {
    const root = window.document.documentElement;
    root.classList.remove("light", "dark");
    
    if (theme === "system") {
      const systemTheme = window.matchMedia("(prefers-color-scheme: dark)")
        .matches ? "dark" : "light";
      root.classList.add(systemTheme);
    } else {
      root.classList.add(theme);
    }
  }, [theme]);
  
  return (
    <ThemeProviderContext.Provider value={{ theme, setTheme }}>
      {children}
    </ThemeProviderContext.Provider>
  );
};
```

## 🔐 Autenticação e Segurança

### Fluxo de Autenticação

1. **Login/Registro**: Supabase Auth UI
2. **Token Storage**: Session gerenciada pelo Supabase
3. **Auto-refresh**: Tokens renovados automaticamente
4. **API Calls**: JWT incluído em todas as requisições

### Proteção de Rotas

```typescript
const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  
  useEffect(() => {
    supabase.auth.getSession().then(({ data: { session } }) => {
      setUser(session?.user ?? null);
      setIsLoading(false);
    });
    
    const { data: { subscription } } = supabase.auth.onAuthStateChange(
      (_event, session) => setUser(session?.user ?? null)
    );
    
    return () => subscription.unsubscribe();
  }, []);
  
  if (isLoading) return <LoadingSpinner />;
  if (!user) return <AuthPage />;
  
  return <>{children}</>;
};
```

## 📱 Responsividade e Acessibilidade

### Design Responsivo

```css
/* Mobile First Approach */
.sidebar {
  @apply w-full h-auto;
}

/* Tablet */
@media (min-width: 768px) {
  .sidebar {
    @apply w-80 h-screen;
  }
}

/* Desktop */
@media (min-width: 1024px) {
  .sidebar {
    @apply w-96;
  }
}
```

### Acessibilidade

- **Navegação por teclado**: Todos os elementos interativos
- **Screen readers**: Textos alternativos e ARIA labels
- **Contraste**: Cores que atendem WCAG 2.1 AA
- **Focus management**: Estados visuais claros

```typescript
// Exemplo de componente acessível
<Button
  aria-label="Gerar resumo da nota"
  disabled={isGenerating}
  onClick={handleGenerateSummary}
>
  {isGenerating ? (
    <LoadingSpinner aria-hidden="true" />
  ) : (
    <Sparkles aria-hidden="true" />
  )}
  <span className="sr-only">
    {isGenerating ? 'Gerando resumo...' : 'Gerar resumo'}
  </span>
</Button>
```

## 🛠️ Desenvolvimento

### Scripts Disponíveis

```bash
npm run dev          # Servidor de desenvolvimento (http://localhost:8080)
npm run build        # Build de produção
npm run build:dev    # Build de desenvolvimento
npm run lint         # Verificação de código
npm run preview      # Preview da build de produção
```

### Estrutura de Desenvolvimento

```typescript
// Configuração do Vite
export default defineConfig({
  plugins: [react()],
  server: {
    port: 8080,
    proxy: {
      '/api': {
        target: 'http://localhost:5181',
        changeOrigin: true
      }
    }
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src')
    }
  }
});
```

### Variáveis de Ambiente

```env
# .env (não commitado)
VITE_SUPABASE_URL=https://your-project.supabase.co
VITE_SUPABASE_ANON_KEY=your-anon-key

# .env.template (template para novos desenvolvedores)
VITE_SUPABASE_URL=your_supabase_project_url
VITE_SUPABASE_ANON_KEY=your_supabase_anon_key
```

## 🧪 Testes e Qualidade

### Estratégia de Testes

```typescript
// Testes de componentes
describe('NoteEditor', () => {
  it('should save note when form is submitted', async () => {
    render(<NoteEditor note={mockNote} onSave={mockSave} />);
    
    fireEvent.change(screen.getByLabelText(/título/i), {
      target: { value: 'Novo título' }
    });
    
    fireEvent.click(screen.getByRole('button', { name: /salvar/i }));
    
    await waitFor(() => {
      expect(mockSave).toHaveBeenCalledWith(
        expect.objectContaining({ title: 'Novo título' })
      );
    });
  });
});
```

### Linting e Formatação

```json
// .eslintrc.json
{
  "extends": [
    "@typescript-eslint/recommended",
    "plugin:react-hooks/recommended"
  ],
  "rules": {
    "react-hooks/exhaustive-deps": "warn",
    "@typescript-eslint/no-unused-vars": "error"
  }
}
```

## 🚀 Build e Deploy

### Build de Produção

```bash
npm run build
```

Gera:
- `dist/` - Assets otimizados
- Code splitting automático
- Assets com hash para cache
- CSS e JS minificados

### Otimizações Implementadas

1. **Code Splitting**: Lazy loading de rotas
2. **Tree Shaking**: Remoção de código não utilizado  
3. **Asset Optimization**: Compressão de imagens e fonts
4. **Bundle Analysis**: Análise de tamanho dos bundles

## 📈 Performance

### Métricas de Performance

- **First Contentful Paint**: < 1.5s
- **Largest Contentful Paint**: < 2.5s
- **Cumulative Layout Shift**: < 0.1
- **First Input Delay**: < 100ms

### Otimizações Implementadas

```typescript
// Lazy loading de componentes
const FlashcardViewer = lazy(() => import('./components/FlashcardViewer'));

// Memoização de componentes pesados
const MemoizedNoteList = memo(NoteList);

// Debounce para auto-save
const debouncedSave = useCallback(
  debounce((note: Note) => {
    apiClient.updateNote(note.id, note);
  }, 1000),
  []
);
```

## 🔄 Roadmap Frontend

### Versão Atual (v1.0)
- ✅ Interface completa e responsiva
- ✅ Sistema de temas
- ✅ Integração com API
- ✅ Funcionalidades de IA

### Próximas Versões
- 🔄 PWA (Progressive Web App)
- 📱 Otimizações mobile
- 🎨 Mais temas e customizações
- ⚡ Performance melhorada
- 🧪 Cobertura de testes completa

---

<div align="center">
  <p>Frontend desenvolvido com as melhores práticas modernas de React</p>
</div>