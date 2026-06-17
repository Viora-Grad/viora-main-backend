# AI RAG Setup Guide

## Prerequisites

- [Qdrant](https://qdrant.tech/) running locally on port `6334` (gRPC)
- `GROQ_API_KEY` environment variable set (with access to `llama-3.1-8b-instant`)
- .NET 10 SDK

---

## 1. Download the ONNX Embedding Model

The local embedding model (`all-MiniLM-L6-v2`) is **not stored in git**. Run the download script:

```powershell
# From Viora/Viora.Api/
./download-models.ps1
```

This fetches `model.onnx` (90 MB) from HuggingFace and places it at:

```
Viora/Viora.Api/Models/all-MiniLM-L6-V2/
├── config.json
├── model.onnx              ← downloaded
├── special_tokens_map.json
├── tokenizer_config.json
├── tokenizer.json
└── vocab.txt
```

> The `Models/` folder is gitignored — model binaries will not be committed.

---

## 2. Knowledge Base & Specialty Data

Download the `Knowledge/` folder from Google Drive:

[https://drive.google.com/drive/folders/16VtZOmS4kzFlURaNThjW2gGHyGMcdY_C](https://drive.google.com/drive/folders/16VtZOmS4kzFlURaNThjW2gGHyGMcdY_C?usp=sharing)

Extract or place the folder at `Viora/Viora.Api/Knowledge/` so the structure looks like:

```
Viora/Viora.Api/Knowledge/
├── viora_knowledge_base.md
└── specialty_inquiries.json
```

| File | Format | Purpose |
|---|---|---|
| `viora_knowledge_base.md` | Markdown | App knowledge base — split by `##` headings into chunks |
| `specialty_inquiries.json` | JSON array | Medical specialty Q&A pairs — `[{ "Category": "...", "Question": "..." }]` |

Both files are copied to the output directory (`PreserveNewest`) and their paths are configured in `appsettings.json` under `AiRag:KnowledgeBase` / `AiRag:SpecialtyBase`.

> The `Knowledge/` folder is gitignored — you must download it manually after cloning.

---

## 3. Controllers

All AI RAG endpoints are under the `/api/ai` route prefix and live in `Viora/Viora.Api/Controllers/AiRag/`.

### 3.1 `ChatsController` — `POST /api/ai/chats`

Main chat endpoint. Sends a user message through intent detection → handler routing → LLM response.

**Request body:**

```json
{
  "message": "What services does Viora offer?",
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

| Field | Type | Required | Description |
|---|---|---|---|
| `message` | string | yes | The user's message |
| `sessionId` | guid | no | Existing session to continue, or omit to start a new one |

**Response:**

```json
{
  "message": "Viora offers mental wellness services...",
  "intent": "KnowledgeQuery",
  "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

| Field | Type | Description |
|---|---|---|
| `message` | string | The assistant's reply |
| `intent` | string | Detected intent: `Greeting`, `General`, `KnowledgeQuery`, `SpecialtyRecommendation`, `Unclear` |
| `sessionId` | guid | Session ID (new or existing) |

### 3.2 `SessionsController` — session management

**`GET /api/ai/sessions`** — list all sessions for the current user.

Query params: `page` (default 1), `pageSize` (default 10, max 50).

Response: paginated list of session summaries (metadata only, no message history).

**`GET /api/ai/sessions/{sessionId:guid}`** — get full message history for a session.

Response: session metadata + array of user/assistant messages.

### 3.3 `IngestionController` — knowledge ingestion

Ingests knowledge base content into Qdrant vector store collections. Requires Qdrant to be running.

**`POST /api/ai/ingestion/knowledge`** — ingest the markdown knowledge base.

Reads the file from `AiRag:KnowledgeBase:FilePath`, chunks by `##` headings, embeds, and upserts into the `viora_knowledge` Qdrant collection.

**`POST /api/ai/ingestion/knowledge/raw`** — ingest raw markdown content.

Request body: raw markdown string.

**`POST /api/ai/ingestion/specialty`** — ingest the specialty inquiries JSON.

Reads the file from `AiRag:SpecialtyBase:FilePath`, embeds, and upserts into the `viora_medical_specialties` Qdrant collection.

**`POST /api/ai/ingestion/specialty/raw`** — ingest raw specialty JSON.

Request body: JSON array of `{ "Category": "...", "Question": "..." }`.

> All ingestion endpoints are idempotent — re-ingesting updates existing points by content hash.

---

## 4. Quick Start

```powershell
# 1. Start Qdrant (Docker)
docker run -d -p 6333:6333 -p 6334:6334 qdrant/qdrant

# 2. Download the embedding model
cd Viora/Viora.Api
./download-models.ps1

# 3. Set your Groq API key
$env:GROQ_API_KEY = "gsk_your_key_here"

# 4. (Optional) Populate Knowledge/ with your files

# 5. Run the API
dotnet run --project Viora/Viora.Api

# 6. Ingest knowledge (first time only)
curl -X POST http://localhost:5000/api/ai/ingestion/knowledge
curl -X POST http://localhost:5000/api/ai/ingestion/specialty

# 7. Start chatting
curl -X POST http://localhost:5000/api/ai/chats `
  -H "Content-Type: application/json" `
  -d '{"message": "Hello, what can you help me with?"}'
```
