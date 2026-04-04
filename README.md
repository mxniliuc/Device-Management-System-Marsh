# Device Management System

A full-stack **device management system**: track phones and tablets (specs, location, assignment to users). The **Angular** frontend talks to an **ASP.NET Core** REST API backed by **MongoDB**. Optional **Ollama** integration drafts short device descriptions from technical specs.

---

## Stack

| Layer | Technology |
|--------|------------|
| API | C#, ASP.NET Core (**net10.0**), JWT authentication, Swagger in Development |
| Data | MongoDB (official C# driver) |
| UI | Angular **19**, standalone components, dev proxy to the API |
| AI (optional) | Ollama-compatible HTTP API |

---

## Features

- **Devices**: CRUD, list with assigned user name, detail view, create/edit with validation.
- **Users**: CRUD for staff profiles (name, role, location).
- **Auth**: Register and login; JWT required for API access.
- **Assignment**: Users can assign/unassign devices to themselves where the API allows it.
- **Search**: `GET /api/devices/search?q=…` uses a MongoDB **text index** on name, manufacturer, processor, and RAM-related tokens; results are ordered by relevance.
- **AI descriptions**: `POST /api/devices/generate-description` calls a configured LLM when `LlmDescription:Enabled` is true.

---

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) compatible with **net10.0**
- [Node.js](https://nodejs.org/) **18+** and npm (for the UI and optional DB scripts)
- [MongoDB](https://www.mongodb.com/try/download/community) **6+** running locally (default `mongodb://127.0.0.1:27017`) or a reachable connection string
- Optional: [Ollama](https://ollama.com) or Docker, for AI-generated descriptions

---

## Configuration (API)

Edit `backend/DeviceManagement/appsettings.json` (use **User Secrets** or environment variables in production).

| Section | Purpose |
|---------|---------|
| `MongoDb:ConnectionString` | MongoDB URI (database name is taken from `DatabaseName`). |
| `MongoDb:DatabaseName` | Database name (e.g. `device_management`). |
| `Jwt:Key` | Signing key — **must be at least 32 characters**; replace the dev placeholder for any shared or production environment. |
| `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpiresMinutes` | JWT validation and lifetime. |
| `LlmDescription:Enabled` | `true` to enable generate-description endpoint. |
| `LlmDescription:BaseUrl` | OpenAI-compatible base URL including `/v1` (default `http://127.0.0.1:11434/v1`). |
| `LlmDescription:Model` | Model tag (e.g. `llama3.2`). |
| `LlmDescription:ApiKey` | Bearer token if your LLM gateway requires it (Ollama local usually empty). |

On first run the API ensures MongoDB indexes needed for search (text index and `ramSearch` backfill for existing documents).

---

## Run locally

### 1. MongoDB

Start MongoDB so `mongodb://127.0.0.1:27017` is available, or set `MongoDb:ConnectionString` to your cluster URI.

### 2. Backend

```bash
cd backend/DeviceManagement
dotnet run --launch-profile http
```

Default HTTP URL: **http://localhost:5084**  
Swagger UI (Development): **http://localhost:5084/swagger**

### 3. Frontend

```bash
cd frontend
npm install
npm start
```

Open **http://localhost:4200**. The dev server proxies `/api` to **http://localhost:5084** (`frontend/proxy.conf.json`).

### 4. Optional: Ollama for AI descriptions

```bash
docker compose -f docker-compose.ollama.yml up -d
docker compose -f docker-compose.ollama.yml exec ollama ollama pull llama3.2
```

Or install Ollama locally and run `scripts/setup-ollama.sh` after `ollama serve`.

---

## Database scripts (optional)

The `database/` package contains idempotent **Node** scripts to create indexes and seed sample data:

```bash
cd database
npm install
# Set MONGODB_URI in .env if needed, e.g. mongodb://127.0.0.1:27017/device_management
npm run create-db
npm run seed-db
```

The running API also maintains its own indexes (including search). If you use both, avoid conflicting business rules (e.g. duplicate device policies) between scripts and app behavior.

---

## Helper scripts

| Script | Description |
|--------|-------------|
| `scripts/setup-ollama.sh` | Pulls the default Ollama model (`OLLAMA_MODEL`, default `llama3.2`). |
| `scripts/seed-search-demo-devices.sh` | Registers a user and posts several sample devices for manual **search** testing (`curl` + `jq` required). Set `BASE_URL`, `SEED_EMAIL`, `SEED_PASSWORD` as needed. |

---

## Tests

**Integration tests** (MongoDB required — local, Docker Testcontainers, or `INTEGRATION_TESTS_MONGO_CONNECTION_STRING`):

```bash
cd backend
dotnet test DeviceManagement.IntegrationTests/DeviceManagement.IntegrationTests.csproj
```

---

## Repository layout

```
backend/DeviceManagement/     # ASP.NET Core API
backend/DeviceManagement.IntegrationTests/
frontend/                     # Angular app
database/src/scripts/         # Optional MongoDB create/seed JS
scripts/                      # Shell helpers (Ollama, search demo seed)
docker-compose.ollama.yml       # Local Ollama in Docker
requirements.txt              # Original assignment specification
```

---

## API overview

- **Auth**: `POST /api/auth/register`, `POST /api/auth/login`
- **Devices**: `GET/POST /api/devices`, `GET/PUT/DELETE /api/devices/{id}`, `GET /api/devices/search?q=…`, `POST /api/devices/generate-description`, assign/unassign routes under `/api/devices/{id}/…`
- **Users**: standard REST under `/api/users`

Use the Development Swagger UI to explore schemas and try authenticated calls with a Bearer token.

---
