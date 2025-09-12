# 🔧 Configuração do Frontend - NotesWise

## 📋 Setup Inicial

### 1. Configurar Variáveis de Ambiente

Copie o arquivo template e configure suas credenciais:

```bash
cp .env.template .env
```

### 2. Editar o arquivo `.env`

Substitua os valores no arquivo `.env`:

```env
VITE_SUPABASE_PROJECT_ID="seu_project_id_aqui"
VITE_SUPABASE_PUBLISHABLE_KEY="sua_chave_publica_aqui"
VITE_SUPABASE_URL="https://seu_project_id_aqui.supabase.co"
```

### 3. Onde encontrar as credenciais do Supabase

1. Acesse [supabase.com](https://supabase.com/dashboard)
2. Selecione seu projeto
3. Vá em **Settings** → **API**
4. Copie:
   - **Project ID**
   - **anon/public key** (para PUBLISHABLE_KEY)
   - **URL** do projeto

### 4. Instalar dependências

```bash
npm install
```

### 5. Iniciar o servidor de desenvolvimento

```bash
npm run dev
```

## ⚠️ Importante

- **NUNCA** commite o arquivo `.env` 
- Use sempre o `.env.template` como base
- Para produção, configure as variáveis no seu host (Vercel, Netlify, etc.)

## 🔒 Segurança

O arquivo `.env` está no `.gitignore` e não será enviado para o repositório.