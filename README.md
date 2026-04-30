# Branch Queue System (FYP demo)

Runnable stack: **ASP.NET Core 8 Web API + SignalR**, **React (Vite) dashboard & simulator**, **Python FastAPI + scikit-learn** for wait-time inference.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS)
- Python 3.9+ with `pip`

## How to run (3 terminals, no Docker)

### Terminal 1 — ML service (port 8000)

```bash
cd ml-service
python3 -m pip install -r requirements.txt
python3 -m uvicorn app:app --host 127.0.0.1 --port 8000
```

If ML is stopped, the API still runs using **heuristic** estimates.

### Terminal 2 — Backend API (port 5000)

```bash
cd backend/QueueSystem.Api
dotnet run
```

- Swagger: `http://localhost:5000/swagger`
- SQLite file: `backend/QueueSystem.Api/queue.db`

### Terminal 3 — Web UI (port 5173, proxies to API)

```bash
cd web
npm install
npm run dev
```

Open **http://localhost:5173**

- Use **Staff dashboard** tab for queue control, crowd/capacity, counters, SignalR live updates.
- Use **Customer simulator** tab to inject tickets (same REST endpoints real clients would use).

## Configuration

| Setting | Location | Purpose |
|--------|----------|---------|
| ML base URL | `backend/QueueSystem.Api/appsettings.json` → `MlService:BaseUrl` | Python inference (`http://localhost:8000`) |
| DB | `ConnectionStrings:Default` | SQLite path (`queue.db`) |

## Branch ID

Seeded branch **`Id = 1`**. The React app uses `BRANCH_ID = 1` in `web/src/types.ts`.

## Features implemented

- Queue join with **daily ticket codes** (`G001`, `C001`, …), capacity block when `occupancy >= MaxCapacity`
- **Crowd level** from occupancy % (Low / Medium / High / Overcrowded)
- Counters: **open/close**, **lunch (staff available)**, **call next**, **complete**, **skip**, **recall**
- **Simulator** bulk endpoint + single-ticket join on simulator page
- SignalR hub **`/hubs/queue`** → `dashboard` payload after each change
- ML **`POST /predict`** → estimated average wait & clearing time (sklearn **Ridge** on synthetic training data for demo)

## Optional later

- Swap SQLite connection string for **SQL Server** when available.
- Add **Redis** for hot-state caching (architecture slide); core logic works without it here.
# FYP2
