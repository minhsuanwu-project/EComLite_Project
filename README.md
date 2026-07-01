# EComLite

A lightweight e-commerce web application built with ASP.NET Core Razor Pages, EF Core, and SQL Server. It includes user authentication (ASP.NET Core Identity), a product catalog, a session-based shopping cart, and order placement/history.

## Features

- **Product catalog** — browse products with SKU, price, currency, and stock quantity
- **Shopping cart** — session-based cart (add, update quantity, remove items)
- **Checkout & orders** — place orders and view order history/details
- **Authentication** — registration and login via ASP.NET Core Identity (email confirmation required; since no external email provider is configured, the confirmation link is shown directly on the registration confirmation page)

## Tech Stack

- ASP.NET Core 8.0 (Razor Pages)
- Entity Framework Core 8.0 + SQL Server
- ASP.NET Core Identity
- xUnit + Moq for tests
- Docker / Docker Compose

## Project Structure

```
EComLite.Web/     Razor Pages web application
EComLite.Tests/   xUnit test project
Dockerfile        Multi-stage build for the web app
docker-compose.yml  Runs the web app + SQL Server together
```

## Run with Docker (recommended)

**Prerequisites:** [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

```bash
docker compose up --build
```

This starts two containers:
- `db` — SQL Server 2022 (Express edition)
- `app` — the EComLite web app, listening on port `8080`

Database migrations are applied automatically on startup, so no manual `dotnet ef database update` step is needed. Once both containers are healthy, open:

```
http://localhost:8080
```

To stop and remove the containers:

```bash
docker compose down
```

To also remove the persisted database volume:

```bash
docker compose down -v
```

## Run Locally (without Docker)

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download) and a local SQL Server instance (e.g. SQL Server Express or LocalDB).

1. Copy the example config and adjust the connection string if needed:
   ```bash
   cp EComLite.Web/appsettings.Development.json.example EComLite.Web/appsettings.Development.json
   ```
2. Restore and run:
   ```bash
   dotnet restore
   dotnet run --project EComLite.Web
   ```
3. The app applies EF Core migrations automatically on startup.

## Running Tests

```bash
dotnet test
```

## Configuration Notes

- The connection string is read from `ConnectionStrings:DefaultConnection`. In Docker, this is supplied via the `ConnectionStrings__DefaultConnection` environment variable in `docker-compose.yml`. Locally, it comes from `appsettings.Development.json` (gitignored — see the `.example` file).
- `appsettings.Development.json`, `*.db`, and `**/secrets.json` are gitignored and never committed.
