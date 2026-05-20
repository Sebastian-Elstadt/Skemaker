# Skemaker

A pipeline that takes a mechanical engineering drawing (PDF or image), extracts its GD&T data with a vision LLM, and translates that analysis into machine-ready G-code.

```
PDF drawing  ──▶  GD&T analysis (JSON)  ──▶  G-code program
                  part name, dims,            strategy summary +
                  callouts, tolerances        full numbered program
```

## A note on intent

This is a curiosity / learning project — built to see how far a vision LLM could be pushed end-to-end, not to ship a production CAM tool. The LLM-driven G-code step is deliberately the wrong tool for the job: real CAM generates toolpaths from a deterministic geometry kernel (parsing a STEP/BREP model, computing offsets, scheduling tool engagement) rather than asking a language model to author motion commands. Doing it this way was the experiment.

As a result, **the generated programs are not useful**. The toolpaths the LLM emits produce a garbage mesh when rendered — the moves are plausible-looking but not geometrically faithful to the drawing, and the strategy choices are not what a real CAM post-processor would produce. A serious version of this would (a) ingest a STEP file alongside the PDF so the model has actual 3D geometry to reason about, and (b) hand the geometry off to a deterministic toolpath generator, using the LLM only for the parts it's good at — reading the drawing, summarizing GD&T callouts, and proposing a high-level strategy.

The rest of this README documents what's here.

## Repository layout

```
.
├── api/                .NET 10 backend (Domain → App → Infra → Api)
├── web/                Angular 21 frontend (lazy-loaded pages, signals)
├── demo/               Sample drawing + example translation request body
├── docker-compose.yml  api + web + postgres for one-command bring-up
└── filestore.local/    Bind-mount target for uploaded drawings (created on first upload)
```

## Pipeline at a glance

| Step | Endpoint | What happens |
|---|---|---|
| 1. Upload | `POST /documents` (multipart) | File is hashed, stored on disk, registered in Postgres. |
| 2. Analyze | `POST /analysis/gdAndT` `{ DocumentId }` | xAI uploads the file once (cached by document id), runs a vision prompt with a strict JSON schema, persists the result. |
| 3. Translate | `POST /translation/gCode` `{ AnalysisId, MachineType, StockSize, Tools, ... }` | xAI re-uses the cached file + analysis JSON to produce a strategy summary, tool list, and full G-code program. |
| 4. View | `GET /translations/{id}` | Web UI shows the program in an editable text pane and renders the toolpath in 3D. |

---

## Backend (`api/`)

Four projects, dependencies point inward only — Domain knows nothing, Api wires everything.

```
Api  ──┐
       ├──▶  App  ──▶  Domain
Infra ─┘
```

### `Domain/` — entities only
Plain C# classes, no framework references. Each entity validates its own invariants in setters and exposes a private `Reconstitute(...)` factory so repositories can rebuild it from row data without going through the constructor.

| File | Purpose |
|---|---|
| `Documents/Document.cs` | Uploaded drawing — filename, path, content type, SHA hash, byte size. |
| `Analysis/DocumentAnalysis.cs` | Stored analysis (`AnalysisJson` is opaque to the domain). |
| `Analysis/DocumentAnalysisType.cs` | Enum: currently `GdAndT`. |
| `Translation/AnalysisTranslation<T>.cs` | Generic translation envelope (`T` = the concrete result, e.g. `GCodeTranslation`). |
| `Translation/AnalysisTranslationTarget.cs` | Enum: currently `GCode`. |

### `App/` — orchestration + abstractions
Application services and the interfaces Infra must satisfy. No concrete IO.

```
App/
├── Abstractions/                          interfaces consumed by services + implemented in Infra
│   ├── IDocumentsService.cs
│   ├── IDocumentAnalysisService.cs
│   ├── IAnalysisTranslationService.cs
│   ├── IFileStore.cs                      blob storage
│   ├── IRecordStore.cs                    Postgres connection + repository roots + transactions
│   ├── IDocumentRepository.cs
│   ├── IDocumentAnalysisRepository.cs
│   ├── IAnalysisTranslationRepository.cs
│   ├── IGdAndTAnalyzer.cs                 the "do the analysis" port
│   └── IGdAndTAnalysisToGCodeTranslator.cs the "translate the analysis" port
├── Documents/DocumentsService.cs          upload → hash → file store → record store
├── Analysis/DocumentAnalysisService.cs    fetch doc → call analyzer → persist
├── Translation/AnalysisTranslationService.cs   fetch analysis → call translator → persist
└── Translation/GCodeManufacturingOptions.cs    parameters from the form (machine, stock, tools, etc.)
```

DTO record types (`DocumentItem`, `DocumentAnalysisItem`, `AnalysisTranslationItem<T>`, `GCodeTranslation`, `DocumentGdAndTAnalysis`) live next to the services that produce them.

### `Infra/` — adapters
Everything that touches the outside world.

```
Infra/
├── FileStore/VolumeFileStore.cs           writes uploads under FileStore:BasePath
├── RecordStore/
│   ├── PostgresRecordStore.cs             IRecordStore implementation (Npgsql + Dapper)
│   ├── PostgresQueryExecutor.cs           thin Dapper wrapper, exposes IQueryExecutor
│   ├── PostgresMigrator.cs                DbUp; runs all embedded .psql at startup
│   └── Migrations/*.psql                  forward-only schema migrations
├── Repositories/                          Document, DocumentAnalysis, AnalysisTranslation
├── Analysis/xAiGdAndTAnalyzer.cs          uploads doc to xAI (cached), runs vision prompt, returns JSON
├── Translation/xAiGdAndTAnalysisToGCodeTranslator.cs   re-uses cached upload, asks for G-code
└── xAI/
    ├── xAiClient.cs                       typed HttpClient wrapper (file upload + responses API)
    ├── xAiConfig.cs                       BaseUrl + ApiKey from config
    └── xAiUploadStore.cs                  infra-internal repo mapping DocumentId → xAiFileId
                                           (so we don't re-upload the same file every step)
```

The `xAiUploadStore` is the architectural answer to "where does the xAI file id go without polluting the domain": it's an infra-local table that no upstream layer knows about. Swap the analyzer for another vendor and the table just becomes dead weight you drop in a migration — `Document` stays clean.

### `Api/` — HTTP surface
Thin controllers, no business logic.

| Route | Verb | Notes |
|---|---|---|
| `/documents` | `GET` | List all documents (newest first in UI). |
| `/documents` | `POST` (multipart) | Upload a drawing; returns `{ DocumentId }`. |
| `/documents/{id}/file` | `GET` | Stream the original file (used by the inline PDF preview). |
| `/analysis/gdAndT` | `POST` `{ DocumentId }` | Trigger a GD&T analysis. Returns the persisted analysis. |
| `/analysis/by-document/{docId}` | `GET` | List analyses for a document. |
| `/analysis/{id}` | `GET` | Fetch one analysis (includes the JSON blob). |
| `/translation/gCode` | `POST` | Trigger a G-code translation; body matches `demo/translate_to_gcode_example.json`. |
| `/translation/by-analysis/{analysisId}` | `GET` | List translations for an analysis. |
| `/translation/{id}` | `GET` | Fetch one translation (includes the program). |

`Program.cs` sets `PropertyNamingPolicy = null` — DTOs travel in PascalCase on the wire. CORS is wide open (dev convenience).

### Build / persistence
- .NET 10. Single `Directory.Build.props` redirects build output to `api/artifacts/`.
- Postgres 18 (alpine image in compose). Schema is rebuilt by DbUp from `Infra/RecordStore/Migrations/*.psql` at startup — no EF Core involved.

---

## Frontend (`web/`)

Angular 21, zoneless change detection, signals throughout, all routes lazy-loaded. No state-management library — page components are self-contained.

```
web/src/app/
├── app.ts / app.html / app.scss          top header shell (brand → /documents, pipeline indicator)
├── app.routes.ts                         lazy routes for the four pages
├── app.config.ts                         provideHttpClient(withFetch()) + zoneless CD
├── core/
│   ├── models/                           TS interfaces mirroring the API DTOs (PascalCase)
│   │   ├── documents.ts                  DocumentItem, CreateDocumentResponse
│   │   ├── analyses.ts                   DocumentAnalysisItem + parsed GdAndTAnalysis (snake_case, comes from xAI)
│   │   └── translations.ts               AnalysisTranslationItem, GCodeTranslation, TranslateToGCodeRequest
│   └── api/                              one service per controller, plain HttpClient
│       ├── documents.api.ts
│       ├── analysis.api.ts
│       └── translation.api.ts
├── pages/
│   ├── documents/                        list + upload
│   ├── document-detail/                  inline PDF preview + analyses list + "Run GD&T analysis"
│   ├── analysis-detail/                  pretty-printed extraction + translation form + translations list
│   └── translation-detail/               editable G-code (left) + 3D toolpath (right) + strategy summary
└── shared/gcode-viewer/
    └── gcode-viewer.component.ts         Three.js scene that consumes @polar3d/gcode-viewer's parser
```

### Page notes
- **Documents** — table sorted by upload date. Inline file picker; PDFs and images accepted.
- **Document detail** — left pane embeds the file via `<iframe>`; right pane lists analyses and triggers new ones.
- **Analysis detail** — overview (part name, material, overall dims), GD&T callouts table, dimensions table, notes. Translation form has every field of `TranslateToGCodeRequest`: machine, material, stock XYZ + unit, work offset, strategy, coolant, safe-Z, and an editable list of tools (add / remove rows). Defaults are seeded from `demo/translate_to_gcode_example.json`. On submit the form collapses and a "Translating… 1–2 minutes" banner takes over.
- **Translation detail** — the G-code is an editable `<textarea>`; keystrokes update the buffer immediately, and a 350 ms debounce pushes the value into the 3D viewer so the toolpath re-renders as you edit. "Reset" reverts to the original loaded text. Copy + download buttons act on the current edit buffer.

### G-code viewer
`@polar3d/gcode-viewer` is only a **parser** — it turns G-code into a `THREE.Group`. The component sets up its own scene, perspective camera (Z-up so the bed sits on the XY plane), `WebGLRenderer`, `OrbitControls`, ambient + directional + fill lighting, axes helper, and floor grid. On first parse it frames the camera to the bounding box; on subsequent re-parses (i.e. user edits) the camera stays put. Geometries and materials are disposed between parses to keep GPU memory flat.

CNC programs only emit `unknown` (cutting moves) and `travel` (rapids) path types, so the viewer overrides `customColors` to cream + brand orange for clear contrast against the dark scene.

### Build / serve
- Dev: `npm install && npm start` — serves on `http://localhost:4200`, talks to `http://localhost:8000`.
- Prod: `npm run build` — output in `dist/Skemaker/browser/`. The included `Dockerfile` builds and serves via nginx; `nginx.conf` does SPA fallback + long-cache fingerprinted assets.

---

## Running the whole stack

`docker-compose.yml` brings up the api on `127.0.0.1:8000`, the web on `127.0.0.1:8001`, and Postgres on an internal network.

It expects an `.env` (next to `docker-compose.yml`) with:

```env
DB_CONNECTION_STRING=Host=postgres;Username=...;Password=...;Database=...
POSTGRES_USER=...
POSTGRES_PASSWORD=...
POSTGRES_DB=...
XAI_BASE_URL=https://api.x.ai
XAI_API_KEY=...
```

The compose file references an external Docker network (`skemaker`) and an external volume (`skemaker_postgres`); create them once:

```bash
docker network create skemaker
docker volume create skemaker_postgres
docker compose up --build
```

The API runs migrations on boot. Open the web UI at `http://localhost:8001`.

### Running pieces individually
- **API only** — `cd api && dotnet run --project Api` (set `RecordStore__ConnectionString`, `FileStore__BasePath`, and the `xAi__*` env vars in `Api/appsettings.Development.json` or via environment).
- **Web only** — `cd web && npm install && npm start`. Talks to `http://localhost:8000` (see `web/src/environments/environment.ts`).

---

## Demo assets (`demo/`)
- `nist_ftc_07_asme1_rd.pdf` — NIST test drawing useful for sanity-checking the analyzer end-to-end.
- `translate_to_gcode_example.json` — full example request body for `POST /translation/gCode`; these are the values the web form pre-populates.

---

## Tech stack

| Layer | What |
|---|---|
| Backend | .NET 10, ASP.NET Core controllers, Dapper, Npgsql, DbUp |
| Database | Postgres 18 |
| LLM | xAI (Grok responses API), vision + structured JSON output, file upload for re-use across calls |
| Storage | Local volume bind-mount for uploaded drawings |
| Frontend | Angular 21 (zoneless, signals, lazy routes), Three.js + `@polar3d/gcode-viewer` |
| Packaging | Multi-stage Dockerfiles for both api and web; docker-compose for orchestration |
