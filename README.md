# site-plan-generator

AI-assisted site plan generation for US Hunger meal-packing events.

A project manager enters a venue's details and uploads its architectural floor plan. The system asks GPT-5.1 for a written drafting spec, feeds that spec plus the uploaded plan to an image model, and returns a site plan drawing with assembly-line tables, pallet storage, stage placement, and volunteer/supply flow arrows drawn on top of the original architecture. The PM reviews the draft, requests edits in plain English, and finalizes the version they want.

This README is written for whoever picks the project up next. It covers how to run it, how the pieces fit, what is deployed where, and — importantly — the list of things that are not quite bugs yet but will be.

---

## Table of contents

1. [Architecture](#architecture)
2. [Repo layout](#repo-layout)
3. [Running locally](#running-locally)
4. [Database](#database)
5. [API reference](#api-reference)
6. [How the AI pipeline works](#how-the-ai-pipeline-works)
7. [What is deployed, and where](#what-is-deployed-and-where)
8. [Known issues and landmines](#known-issues-and-landmines)
9. [Paths for expansion](#paths-for-expansion)
10. [Conventions](#conventions)

---

## Architecture

Three pieces, all on one Windows EC2 instance:

| Piece | Tech | Location |
|---|---|---|
| Frontend | React 19 + TypeScript + Vite + Tailwind 4 + shadcn (Base UI) | `frontend/` |
| Backend | ASP.NET Web API 2 on .NET Framework 4.7.2, Entity Framework 6 (database-first, EDMX) | `spGenerator/` |
| Database | SQL Server LocalDB (see the caveat in [Known issues](#known-issues-and-landmines)) | `SitePlanProd` |

External services: **OpenAI** (`gpt-5.1` for text, `gpt-image-1` for images) and **iLovePDF** (converts uploaded PDF floor plans to PNG, since the image API only accepts images).

The frontend is a static bundle. There is no Node process in production — IIS serves the compiled files the same way it serves an image.

---

## Repo layout

```
site-plan-generator/
├── frontend/
│   └── src/
│       ├── App.tsx              New request form (the / route)
│       ├── Past.tsx             Venue list (/past)
│       ├── Request.tsx          Request detail, drafts, edit panel (/requests/:id)
│       ├── image.tsx            Generated draft image viewer (/requests/:id/image)
│       ├── ogBlueprint.tsx      Uploaded floor plan viewer (/requests/:id/image/og)
│       ├── sidebar.tsx          Nav rail
│       ├── lib/routes.tsx       Route table
│       └── components/ui/       shadcn components (Base UI based, not Radix)
└── spGenerator/spGenerator/
    ├── Controllers/
    │   ├── RequestController.cs        CRUD + file upload
    │   ├── ReviewController.cs         Drafts, edits, finalize
    │   └── SitePlanPromptBuilder.cs    POST /api/prompt — triggers generation
    ├── Services/
    │   ├── ReqServices.cs              Request CRUD, file writes
    │   ├── SitePlanPromptServices.cs   Initial generation + GeneratePrompt()
    │   └── ReviewServices.cs           Draft edits, finalize
    ├── Model1.edmx                     EF database-first model
    ├── blueprints/                     Generated PNGs
    │   └── ogInput/                    Uploaded venue plans
    └── context/
        ├── before/                     Blank venue floor plans
        └── after/                      The same venues, finished by hand
```

`context/before` and `context/after` are **training examples**, not sample data. Files with matching names are the same venue before and after a human drafted the site plan. They are attached to every text-model call so it can learn the transformation. Do not delete them — generation throws if the folders are empty.

---

## Running locally

### Prerequisites

- Visual Studio 2022 with the ASP.NET and web development workload
- Node 20+
- SQL Server (LocalDB is what the project currently uses)
- Environment variables `OPENAI_API_KEY` and `ILOVEPDF_KEY`

### Backend

1. Open `spGenerator/spGenerator.slnx` in Visual Studio.
2. Check the connection string in `Web.config` points at a database you can reach.
3. F5. It runs under IIS Express, typically at `https://localhost:44306`.

The first launch will prompt to trust the IIS Express development certificate — accept it, or the frontend's fetch calls fail with an opaque network error.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Runs at `http://localhost:5173`.

**Important:** `apiBase` is hardcoded at the top of five files (`App.tsx`, `Past.tsx`, `Request.tsx`, `image.tsx`, `ogBlueprint.tsx`). In production it is `'/spGenerator'` (relative, same origin). For local development against IIS Express it needs to be `'https://localhost:44306'`. Either flip it while developing, or add a proxy to `vite.config.ts` so one value works in both places:

```ts
server: {
  proxy: {
    '/spGenerator': {
      target: 'https://localhost:44306',
      changeOrigin: true,
      secure: false,
      rewrite: (path) => path.replace(/^\/spGenerator/, ''),
    },
  },
},
```

Note the build script is `vite build` only — the TypeScript check was removed from it because the codebase has type errors that block compilation. Run `npx tsc -b` yourself to see them; they are all in the "should fix" pile, not the "will crash" pile.

---

## Database

Four tables. Names are **singular** (`SitePlanDraft`, not `SitePlanDrafts`) — the plural forms you see in C# are EF entity *sets*.

### SitePlanRequest — what the PM submits

| Column | Type | Notes |
|---|---|---|
| Id | int identity | PK |
| ClientName | nvarchar(500) | |
| VenueName | nvarchar(500) | |
| VenueAddress | nvarchar(500) | Assembled client-side from address1/2, city, state, zip |
| Deadline | datetime NULL | **Nullable, and the frontend does not guard for null** |
| NumberofLines | int NULL | Assembly lines requested |
| NumberofPalettes | int NULL | |
| TableSizes | int NULL | Table **length in feet**, not seats. Single value only |
| RoomDimensions | nvarchar(max) | Free text, e.g. "180ft x 120ft clear span" |
| LoadingNotes | nvarchar(max) | |
| AdditionalNotes | nvarchar(max) | |
| RoomBlueprintFilePath | nvarchar(500) | Filename only, resolved against `blueprints/ogInput/` |
| CreatedAtUtc | datetime | Defaults to `getutcdate()` |

### SitePlanDraft — one AI generation

| Column | Type |
|---|---|
| Id | int identity |
| SitePlanRequestId | int, FK → SitePlanRequest |
| SiteOverview | nvarchar(max) — relative path to the generated PNG |
| RecommendedLayout, Timeline, Risks, PMReviewChecklist | nvarchar(max) |
| CreatedAt | datetime |
| Reviewer | varchar(500) — currently always `"Placeholder Name"` |
| AdditionalNotes | nvarchar(max) |
| finalVersion | int NULL, FK → SitePlanFinal |

**This table has no `VolunteerFlow` or `SupplyFlow` columns**, even though the generation schema asks the model for both and `SitePlanFinal` has them. Those two fields are generated, paid for, and silently discarded. See [Known issues](#known-issues-and-landmines).

### SitePlanFinal — an approved draft

Same shape as `SitePlanDraft` plus `VolunteerFlow`, `SupplyFlow`, and `ApprovedAt` (never set by the code), minus `finalVersion`.

### ReferenceDocument

Exists in the schema, unused by the application.

`getAllRequestDrafts` returns drafts **newest first** (`OrderByDescending(CreatedAt)`). Several frontend bugs come from code that assumes the opposite.

---

## API reference

Base path in production: `/spGenerator`. All responses are JSON.

### Requests — `RequestController.cs`

| Method | Route | Body | Notes |
|---|---|---|---|
| POST | `/api/requests` | `SitePlanRequest` | Returns the created row with its new `Id` |
| GET | `/api/requests/{id}` | — | 404 if not found |
| GET | `/api/requests/getAll` | — | Ordered by `Deadline` descending |
| POST | `/api/requests/uploadFile` | multipart, field name `file` | Writes to `blueprints/ogInput/` |

### Review — `ReviewController.cs`

| Method | Route | Body | Notes |
|---|---|---|---|
| GET | `/api/review/{id}/draft` | — | One draft by draft id |
| GET | `/api/review/getAllRequestDrafts/{reqId}` | — | All drafts for a request, newest first |
| PUT | `/api/review/{id}/draft/edit` | JSON string of the edit notes | Runs a full generation; takes 1-3 minutes |
| POST | `/api/review/{id}/finalize` | — | Copies the draft into `SitePlanFinal` |

### Generation — `SitePlanPromptBuilder.cs`

| Method | Route | Body | Notes |
|---|---|---|---|
| POST | `/api/prompt/` | the full `SitePlanRequest` object | Creates the initial draft. 1-3 minutes |

Generated and uploaded images are served as static files from `/spGenerator/blueprints/...`, since those folders live inside the deployed application.

---

## How the AI pipeline works

This is the part worth understanding before changing anything.

**The image model cannot see the before/after examples.** The images API accepts one input image and a text prompt — that is all. So the examples go to the *text* model, which distills them into a field called `ImageInstruction`, and that string is what the image model receives. Every improvement to drawing quality has to travel through that string.

### Initial generation — `SitePlanPromptServices.askAI()`

1. Load every file from `context/before` and `context/after` as attachments.
2. Call `gpt-5.1` with a JSON schema requiring `SiteOverview`, `RecommendedLayout`, `VolunteerFlow`, `SupplyFlow`, `Timeline`, `Risks`, `PMReviewChecklist`, and `ImageInstruction`.
3. If the request has an uploaded plan, convert it to PNG (via iLovePDF when it is a PDF) and call `GenerateImageEditAsync` with that PNG as the base image and `GeneratePrompt(req, null, true, "", imageInstructions)` as the prompt. Options: `Size = W1024xH1536`, `Quality = HighQuality`, `InputFidelity = High`.
4. If there is no uploaded plan, fall back to `GenerateImageAsync` — which draws a generic rectangular room and looks nothing like a real venue. Always upload a plan.
5. Save the PNG to `blueprints/`, deserialize the JSON into a `SitePlanDraft`, save the row.

### Edits — `ReviewServices.editDraft()`

Same shape, with three differences that matter:

- The base image is the **previous draft's PNG**, not the original venue plan. Each edit is a repaint of a repaint, so architectural fidelity decays with every round.
- The prompt is a hand-built string (`editPrompt`), **not** `GeneratePrompt`. All the drawing rules — table geometry, scale anchoring, colors, "never draw a table as a square" — live in `GeneratePrompt`'s image branch and never reach the edit path.
- The edit's JSON schema omits `VolunteerFlow` and `SupplyFlow`.

### `GeneratePrompt(req, draft, image, edits, context)`

One function, four modes, in `SitePlanPromptServices.cs`. When `image` is `true` it returns early with the long drawing-rules prompt (preserve all architecture, scale anchoring, table proportions, color conventions, spelling). When `image` is `false` it builds the text-model prompt. `req == null` means "this is an edit of an existing draft."

Read this function before touching prompt behavior. It is the center of the system.

---

## What is deployed, and where

Everything runs on the EC2 instance `EC2AMAZ-R50QG1E`.

| Thing | Value |
|---|---|
| IIS site | `siteplan`, port 8080 |
| Frontend | `C:\Deploy\siteplan-web` (contents of `frontend/dist`) |
| API application | `/spGenerator` → `C:\Deploy\site-plan-generator` |
| App pool | `sitePlanAI`, .NET v4.0, Integrated, ApplicationPoolIdentity, idle timeout 0, recycling off |
| Database | `SitePlanProd` on LocalDB, shared instance name `wcfRESTShare` |
| Connection string | `data source=(localdb)\.\wcfRESTShare;initial catalog=SitePlanProd;integrated security=True` |
| API keys | Environment variables on the app pool (`OPENAI_API_KEY`, `ILOVEPDF_KEY`) in `applicationHost.config` |
| Uploads | `C:\Deploy\site-plan-generator\blueprints\ogInput` |
| Generated images | `C:\Deploy\site-plan-generator\blueprints` |

Port 80 is taken by an unrelated WCF project (`Default Web Site/api` → `C:\inetpub\wwwroot\WebApplication3`). Leave it alone.

### Redeploying the backend

Publish from Visual Studio: Release configuration, Folder target, `C:\Deploy\site-plan-generator`, with **"Remove additional files at destination" unchecked** — that checkbox deletes every uploaded plan and generated draft.

Two things do not publish automatically and must be checked each time:
- `Global.asax` (its Build Action is not Content — without it, every API route 404s)
- the `context/` folders

### Redeploying the frontend

```bash
cd frontend
npm run build
xcopy /E /I /Y dist "C:\Deploy\siteplan-web"
```

`apiBase` is compiled into the bundle, so it must be `'/spGenerator'` before you build.

### Useful commands

```powershell
$appcmd = "$env:windir\system32\inetsrv\appcmd.exe"
& $appcmd list sites
& $appcmd recycle apppool /apppool.name:"sitePlanAI"
curl.exe http://localhost:8080/spGenerator/api/requests/getAll
sqllocaldb info MSSQLLocalDB
```

---

## Known issues and landmines

Roughly in order of how much they will hurt.

### Will break in normal use

**Upload rejects every file.** `RequestController.cs` compares `Path.GetExtension(file.FileName)` against `"pdf"` — but `GetExtension` returns `".pdf"` with the dot. Every upload returns "Invalid file type," which means every request silently falls back to the draw-from-scratch path and produces a generic room. Fix: compare against `".pdf"` etc. and call `.ToLower()`.

**A request with no deadline blanks two pages.** `Past.tsx` and `Request.tsx` both call `.slice(0, 10)` on `Deadline`, which is nullable and not required by the form. One dateless request and the venue list renders white for everyone.

**A null draft field blanks the request page.** `textToList` calls `.includes` on the text without a null check. Any draft where the model omitted a field throws.

**The Select Draft box throws on most input.** It is bound to `value={drafts.length}` (a constant, so typing does nothing visible) and indexes `drafts[Number(value) - 1]`. Clearing the box gives `drafts[-1]` → `undefined` → `.Id` throws. It is also inverted: the array is newest-first while the labels count up, so typing "2" selects the draft labeled "1".

**Edits target the wrong draft.** `setDraftEditID(drafts[drafts.length - 1].Id)` picks the *oldest* draft, because the API returns newest first. The UI text promises the most recent one. Should be `drafts[0].Id`.

**LocalDB stops when nobody is logged in.** It runs inside the Administrator session and has no Windows service. After a reboot the API returns "The underlying provider failed on Open" until someone logs in or a scheduled task runs `sqllocaldb start MSSQLLocalDB`. The instance also has no SQL Agent, so backups need Task Scheduler. Moving to the full SQL Server instance on the same box removes this whole class of problem and only changes `data source`.

### Quietly wrong

**Two AI fields are thrown away.** `SitePlanDraft` has no `VolunteerFlow`/`SupplyFlow` columns while the generation schema requires them. You pay for them every call and lose them. Fix is an `ALTER TABLE SitePlanDraft ADD VolunteerFlow nvarchar(max) NULL, SupplyFlow nvarchar(max) NULL;`, an EDMX refresh, and adding both to the edit schema in `ReviewServices.cs`.

**Edits lose the drawing rules and run at default fidelity.** `editPrompt` bypasses `GeneratePrompt`, and `GenerateImageEditAsync` in `ReviewServices.cs` passes no `ImageEditOptions` — so edits render at 1024×1024 square with default input fidelity while the initial generation uses `W1024xH1536` with `InputFidelity.High`. This is the main reason edited drafts lose the building's walls.

**Uploaded files can overwrite each other.** The saved name is derived from the client's filename. Two people uploading `floorplan.pdf` collide, and both requests then point at the same image. A GUID or the request id in the name fixes it. Generated PNGs use a to-the-second timestamp, which collides only under concurrent use.

**Nothing refreshes after an action.** Generate, edit, and finalize all `console.log` and stop. The user sees no change, assumes failure, and clicks again — each click is another paid image generation. `await getDrafts()` before clearing the loading state fixes it.

**Finalize does nothing.** The button in `Request.tsx` has no `onClick`; `finalizeDraft` is defined and never called. There is also no page for finalized drafts.

**The venue list shows skeletons forever when empty.** `Past.tsx` picks skeletons whenever `drafts.length === 0`, which is also the state on a fresh database and after a failed fetch. It needs a separate "loaded" flag.

**Measurements in the generated drawings are decorative.** Diffusion models cannot do arithmetic or reliable lettering. Draft output has included a stage labeled `2'-0"` that should be 24', dimension arrows that disagree with each other, and misspellings like "ENTRR". Do not let anyone tape a floor from these numbers.

### Production hygiene

- **No authentication.** Anyone who can reach the site can read every client and spend OpenAI credits. Windows Authentication in IIS is the cheap fix for an internal tool, and it would also let `Reviewer` be `User.Identity.Name` instead of a placeholder.
- **No logging.** Failures go to the browser and nowhere else. `ex.Message` is returned to the client, which leaks file paths and SQL text.
- **The database backup does not protect the images.** `SiteOverview` is only a path; the PNGs live on disk. Back up `C:\Deploy\site-plan-generator\blueprints` too.
- **`DbContext` is never disposed.** Each service constructs one per request and holds it. Fine at current volume, connection-pool exhaustion later.
- **The iLovePDF project id is hardcoded** in two files while its key comes from an environment variable.
- **Multi-page PDFs break conversion.** `PdfToJpgModes.Pages` returns a zip for multi-page documents, and `Image.FromStream` throws on it. Venue plans are often multi-page — either restrict uploads to single-page exports or unpack the archive.
- **Country is collected on the form and never sent.**

---

## Paths for expansion

**Render the overlay instead of generating it.** The highest-value change available. The finished plans this project imitates are the original CAD drawing with a vector overlay — grey table rectangles, red electricity chains, brown pallet blocks, real dimension text. An image model cannot produce that, and no amount of prompt work will get it there. Have the model return **coordinates** instead of a picture — table rectangles as x/y/width/height in feet, flows as polylines, zones as boxes — all in the JSON schema that already exists, then draw them as SVG over the uploaded plan. Architecture is untouched because you never redraw it, tables are exactly 2.5×6 because you draw them, and labels are real text. The pipeline stays the same; only the last step changes.

**A structured placement table.** A smaller version of the same idea and a good first step: add fields to the JSON schema for line count, table count, and positions described against named features ("Line 3 begins 12 ft east of the stage"), then show that beside the image. Gives PMs numbers they can verify without touching the image path.

**Finalized drafts page.** The table, the route, and the sidebar link are all stubbed. `finalizeDraft` already works server-side.

**Edit history.** Every edit creates a new draft row, but there is no way to see what was asked for — the edit notes are not persisted. Storing them per draft would make the accordion a real revision log.

**More context examples.** Currently three before/after pairs, all similar in venue shape. The model has no example of curved walls, structural columns mid-floor, L-shaped rooms, or very small venues, and it forces its standard grid onto them. Six to eight pairs chosen for *variety* would help more than fifteen similar ones. Higher leverage still: write a short caption for each pair describing what changed and why — right now the model has to infer the whole transformation from raw file pairs.

**Cost controls.** Disable the generate/edit buttons while a request is in flight, and consider a per-user daily cap. Each click is a real charge.

---

## Conventions

- **Minimal, surgical edits.** The codebase favors small changes to existing lines over refactors — flat `StringBuilder.Append` calls, explicit `!= null && != ""` checks, `if/else` with braces, no extracted helpers. Match what is around you.
- **`apiBase` is relative in production** (`/spGenerator`) so the app follows whatever host and scheme the browser used. Never hardcode a hostname.
- **shadcn components here are Base UI based, not Radix.** Triggers take a `render` prop rather than `asChild`, and **accordions cannot nest** — inner triggers register with the outer root and never open. Use a local disclosure component for nested sections.
- **Draft ordering is newest-first everywhere.** `drafts[0]` is the most recent.
- **Prompt changes are the product.** Most output-quality work happens in `GeneratePrompt` and the JSON schemas, not in C# logic.
