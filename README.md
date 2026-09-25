# TripCraft

TripCraft is a Sri Lankan inbound-tour operations platform built with React, ASP.NET Core 8, EF Core and PostgreSQL. Tourists submit custom trip objectives, Operations Managers manage resources and quotations, Guides receive assigned schedules, and Admins manage users and roles.

## Included

- Responsive login and Tourist-only public registration
- JWT access/refresh tokens, BCrypt password hashing and role-based authorization
- PostgreSQL entities for tourists, trip requests, itineraries, attractions, guides, vehicles, hotels, rooms, quotations, approvals, holds and audit logs
- Tourist trip submission and status tracking
- Operations dashboard, resource inventory, quotation calculation and approval
- Admin-only user and role management
- Swagger/OpenAPI and RFC 7807 errors

The planning endpoint currently persists an approval-gated workflow boundary and itinerary skeleton. The Python LangGraph/Ollama agent service, Flutter mobile client and third-party weather/distance/FX clients remain the next implementation phase.

## Run locally

Prerequisites: Node.js, .NET 8 SDK, Docker Desktop.

```bash
docker compose up -d postgres
dotnet run --project backend/WorkForce360.Api --launch-profile http
npm install
npm run dev
```

Open `http://localhost:5173`. Swagger is at `http://localhost:5170/swagger`.

Development accounts:

- Tourist: `tourist@tripcraft.local` / `Tourist123!`
- Operations Manager: `ops@tripcraft.local` / `Ops12345!`
- Admin: `admin@tripcraft.local` / `Admin123!`

Change the database password, JWT key and demo credentials before deployment. Production should use EF Core migrations instead of automatic database creation.

Set `VITE_API_URL` in `.env.local` to point the web app at a different API.

## Verify

```bash
npm run build
dotnet build WorkForce360.sln
dotnet test WorkForce360.sln
```
