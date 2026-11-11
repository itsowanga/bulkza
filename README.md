# BulkZA

A bulk-buying platform where merchants pool their orders to reach volume discounts. Each merchant pays individually.

This README describes what the code does today. Planned work is listed under [Roadmap](#roadmap).

---

## Table of contents

1. [Problem](#problem)
2. [Solution](#solution)
3. [Architecture](#architecture)
4. [Technologies](#technologies)
5. [Project structure](#project-structure)
6. [Domain model](#domain-model)
7. [BulkPool state machine](#bulkpool-state-machine)
8. [Pricing](#pricing)
9. [API](#api)
10. [Transactional outbox](#transactional-outbox)
11. [Local setup](#local-setup)
12. [Configuration](#configuration)
13. [Testing](#testing)
14. [Roadmap](#roadmap)

---

## Problem

Small merchants in South Africa often can't order enough to qualify for wholesale pricing. BulkZA lets them pool demand on a product. When the pool is big enough, everyone gets the lower price.

## Solution

- Merchants register, log in, and browse active products.
- A merchant creates a bulk pool for a product, or joins an open one.
- When a pool reaches its minimum quantity and minimum merchant count, it moves to `ThresholdMet`.
- A threshold-met pool can be locked, then finalised.
- Each merchant has an individual order with its own payment, shipment, and delivery steps.

---

## Architecture

Dependencies point inward:

```
Api, Worker
    ↓
Application
    ↓
Domain

Infrastructure → Application → Domain
```

| Layer | Responsibility |
|-------|----------------|
| **Domain** | Entities, value objects, the `BulkPool` state machine, domain events, the pricing strategy. No infrastructure dependencies. |
| **Application** | One handler class per use case (command or query), plus interfaces for repositories, the unit of work, JWT, password hashing, the payment gateway, and the event bus. |
| **Infrastructure** | EF Core with SQL Server, repositories, JWT provider, BCrypt password hasher, mock payment gateway, outbox persistence. |
| **Api** | ASP.NET Core controllers, JWT bearer authentication, global exception handler. |
| **Worker** | .NET Generic Host with a background service that publishes outbox messages. |

Handlers are plain scoped classes. There is no mediator library.

---

## Technologies

- .NET 10, C# latest, nullable reference types on
- ASP.NET Core Web API with controllers
- Entity Framework Core 10 with SQL Server
- JWT bearer authentication
- BCrypt.Net-Next for password hashing
- libphonenumber-csharp for phone number validation
- Docker Compose (SQL Server 2025 Developer)
- xUnit, FluentAssertions, Testcontainers (SQL Server), Respawn, `Microsoft.AspNetCore.Mvc.Testing`

Package versions are centralised in `Directory.Packages.props`. Build output goes to `build/`.

---

## Project structure

```
src/
    Domain/
    Application/
    Infrastructure/
    Api/
    Worker/

web/                       (React app, see web/README.md)
scripts/                   (local database setup and seed scripts)

tests/
    Domain.Tests/
    Api.Tests/
    Worker.Tests/
    Application.Tests/     (placeholder)
    Integration.Tests/
```

---

## Domain model

### Value objects

- Identifiers: `MerchantId`, `ProductId`, `PoolId`, `OrderId`, `CartId`, `EventId`
- `Address`, `PhoneNumber`, `PasswordHash`
- `Money`, `Quantity`
- `CartItem`, `OrderLine`, `Participant`
- `DiscountTier` (minimum quantity and discount percentage)

Value objects validate their own invariants.

### Entities and aggregates

| Type | Notes |
|------|-------|
| `Merchant` | Has a role (`Merchant`, `Wholesaler`) and a status (`Pending`, `Active`, `Suspended`). |
| `Product` | SKU, name, optional description, category, unit of measure, base price, active flag. |
| `BulkPool` | Aggregate root. Holds participants and raises domain events. |
| `Cart` | Holds `CartItem` values. |
| `Order` | Holds `OrderLine` values, a payment id, and a refund id. |
| `OutboxMessage` | Persisted integration event awaiting publication. |

Product categories: `MaizeMeal`, `CookingOil`, `Rice`, `Sugar`, `SoftDrinks`, `Flour`.
Units of measure: `Each`, `Kilogram`, `Litre`, `Box`, `Bag`.

### Order status

```
Pending → Paid → Shipped → Delivered
```

An order can be cancelled from `Pending` or `Paid`. Cancelling requires a refund id.

### Domain events

Raised by `BulkPool`:

- `MerchantJoinedPool`
- `MerchantLeftPool`
- `PoolThresholdReached`
- `PoolLocked`
- `PoolFinalized`
- `PoolExpired`
- `PoolCancelled`

---

## BulkPool state machine

A new pool starts in `Open`.

```
Open → ThresholdMet → Locked → Finalized
```

Terminal alternatives: `Expired` and `Cancelled`.

| Method | Allowed from | Effect |
|--------|--------------|--------|
| `Join` | `Open`, before the deadline | Adds the merchant and quantity. Moves to `ThresholdMet` when both thresholds are met. A merchant can join only once. |
| `Leave` | `Open`, `ThresholdMet`, before the deadline | Removes the merchant. Returns to `Open` if a threshold is no longer met. |
| `Lock` | `ThresholdMet` | Moves to `Locked`. |
| `Finalise` | `Locked` | Moves to `Finalized`. |
| `Expire` | `Open`, `ThresholdMet`, after the deadline | Moves to `Expired`. |
| `Cancel` | `Open`, `ThresholdMet` | Moves to `Cancelled`. |

Invalid transitions throw `InvalidOperationException`. Status has a private setter, so changes only happen through these methods.

Creation rules: the deadline must be in the future and the minimum merchant count must be at least one.

---

## Pricing

`IPricingStrategy` has one implementation, `VolumePricingStrategy`. It takes a list of `DiscountTier` values. For a quantity, it applies the highest discount percentage whose minimum quantity is met, then returns the discounted unit price as `Money`.

The strategy is a domain class with unit tests. No handler uses it yet, and it is not registered in dependency injection.

### Relationship with bulk pools and orders

Today the three parts are separate:

- `BulkPool` tracks quantity, participants, thresholds, and the deadline. It has no price and no reference to a pricing strategy.
- `Order` takes `OrderLine` values that already carry a `UnitPrice`. The caller supplies that price. Nothing calculates it.
- `FinalisePoolHandler` only finalises the pool and saves. It does not create orders.
- `CreateOrderHandler` is registered but has no caller, so the API cannot create orders yet.

The intended design connects them when a pool finalises:

```
Pool finalises
      ↓
CurrentQuantity (the pool total) and the product BasePrice go into CalculateUnitPrice
      ↓
The resulting unit price applies to every participant
      ↓
One Order per Participant, with an OrderLine of (ProductId, participant quantity, unit price)
```

The tier comes from the pool total, not from each merchant's own quantity. That is what gives small merchants access to wholesale pricing.

Work needed to build this:

1. Supply real discount tiers, from configuration or stored on `Product`, and register the strategy.
2. Give `FinalisePoolHandler` the product base price, the strategy, and order creation.
3. Create the orders in the same transaction as the pool change.

---

## API

All routes sit under `/api`. Every route requires a bearer token except register and login. Default local URLs are `http://localhost:5132` and `https://localhost:7224`. Sample requests are in `src/Api/Api.http`.

| Controller | Routes |
|------------|--------|
| `merchants` | `POST register`, `POST login` |
| `products` | `GET browse`, `GET {id}`, `GET search/name`, `GET search/sku` |
| `carts` | `GET`, `POST items`, `PUT items/{productId}`, `DELETE items/{productId}`, `DELETE clear` |
| `bulkpools` | `POST`, `GET {id}`, `GET mine`, `GET open?productId=`, `POST {id}/join`, `POST {id}/leave`, `POST {id}/lock`, `POST {id}/finalise`, `POST {id}/expire`, `POST {id}/cancel` |
| `orders` | `GET mine`, `POST {id}/pay`, `POST {id}/cancel`, `POST {id}/ship`, `POST {id}/deliver` |
| `admin` | `GET pools`, `GET orders`, `GET summary` (Admin only) |

Conventions:

- Controllers use request and response DTOs, not domain entities.
- Errors return RFC 7807 `ProblemDetails` through `GlobalExceptionHandler`.
- The merchant id comes from JWT claims, not the request body.
- JSON is camelCase and enums are strings, for example `"status": "ThresholdMet"`. The `category` filter on `GET products/browse` accepts a name such as `Rice`.
- `GET products/search/name` matches the start of a product name, not any part of it.
- Exception mapping: `ArgumentException` and `InvalidOperationException` return 400, `UnauthorizedAccessException` and `AuthenticationException` (failed login) return 401, `KeyNotFoundException` returns 404, anything else returns 500. A 500 response carries a generic message, not the exception text.

Middleware order: exception handler, authentication, authorization, controllers.

### Roles

Every merchant has a role: `Merchant`, `Wholesaler` or `Admin`. The role is a claim in the JWT, so a role change takes effect at the next login.

- Pool `lock`, `finalise`, `expire` and `cancel` need the `Admin` role. Other merchants get 403.
- Order `ship` and `deliver` need the `Admin` role.
- Order `pay` and `cancel` work only on the caller's own orders.

- `GET /api/admin/pools`, `/orders` and `/summary` need the `Admin` role. Anyone else gets 403, and a missing token gets 401.

The admin routes list pools and orders across every merchant, optionally filtered with `?status=`, and `summary` counts both by status. Shipping and delivering an order, and locking or finalising a pool, use the existing action routes listed above.

Registration always creates a plain `Merchant`. There is no endpoint to promote a merchant.

### Creating the first admin

The Api creates one Admin at startup when both `AdminSeed:Phone` and `AdminSeed:Password` are set. Put them in user secrets or environment variables, never in `appsettings.json`:

```bash
dotnet user-secrets set "AdminSeed:Phone" "<South African phone number>" --project src/Api
dotnet user-secrets set "AdminSeed:Password" "<password>" --project src/Api
```

In deployment use the environment variables `AdminSeed__Phone` and `AdminSeed__Password`. Optional keys: `AdminSeed:Name` (default `Administrator`) and the address fields `Street`, `City`, `Province`, `PostalCode` and `Country`, which default to placeholders.

The seeder behaves like this:

- If neither value is set, it does nothing.
- If only one is set, or the phone number is invalid, the Api refuses to start.
- It is safe to run on every start. If the phone number is already registered, it changes nothing. An existing non-admin merchant is never promoted and keeps their password, so use a phone number that is not yet registered.
- It needs the database to be migrated first.
- It logs the new merchant's id, never the password.

Application handlers exist for merchant updates (name, phone number, address, status), product management (create, activate, deactivate, update price, category, description, unit of measure), and order creation. Not every handler has a controller route yet.

Payments go through `IPaymentGateway`. The only implementation is `MockPaymentGateway`, which always succeeds.

---

## Transactional outbox

The Worker is a publisher only. It reads stored outbox messages and hands them to an `IEventBus`. There is no consumer yet.

### Lifecycle

`BulkzaDbContext.SaveChangesAsync` collects the domain events from every tracked `BulkPool`. It writes them as `OutboxMessage` rows in the same save, then clears the events from the aggregate.

```
Handler changes a BulkPool
        ↓
SaveChangesAsync writes the pool and the outbox rows in one transaction
        ↓
OutboxPublisher (Worker) polls the Outbox table for due rows
        ↓
IEventBus.PublishAsync(type, content)
        ↓
success: ProcessedAt set          failure: retry scheduled, or message parked
```

`OutboxMessage` fields: `Id`, `Type` (assembly-qualified event type name), `Content` (the event as JSON), `OccuredAt`, `ProcessedAt`, `Error`, `RetryCount`, `NextAttemptOn`, `ParkedAt`. The table is `Outbox`.

### Polling

Each poll reads up to `BatchSize` messages, oldest first (`OccuredAt`), where:

- `ProcessedAt` is null,
- `ParkedAt` is null, and
- `NextAttemptOn` is null or not later than now.

New messages have a null `NextAttemptOn`, so a null counts as due. If no message was handled, the Worker waits `PollInterval` before polling again. If the poll itself fails, for example because the database is unreachable, it logs the error and waits `ErrorDelay`.

### Publishing

The publisher passes the stored `Type` and `Content` to `IEventBus.PublishAsync(string type, string content, CancellationToken)` exactly as stored. It does not deserialise the JSON, so it can publish any event type, including ones the Worker has no class for.

A message gets `ProcessedAt` only after `PublishAsync` returns without an exception. Messages are published one at a time. One failing message does not stop the rest of the batch.

### Retries and exponential backoff

`MaxRetryCount` is the number of retries after the first attempt. A message is attempted at most `1 + MaxRetryCount` times.

- The first failure sets `RetryCount` to 1 and schedules retry 1 after `InitialRetryDelay`.
- Retry n is scheduled `InitialRetryDelay * 2^(n-1)` after the failure, capped at `MaxRetryDelay`.
- Each failure stores the error text in `Error`.

With the defaults (`MaxRetryCount` 10, `InitialRetryDelay` 5 seconds, `MaxRetryDelay` 5 minutes), the retry delays are 5, 10, 20, 40, 80, 160, then 300 seconds for the rest.

### Parking

When a message fails and no retries are left, the publisher sets `ParkedAt` instead of scheduling another retry. A parked message is never selected again. Its `Error` and `RetryCount` stay on the row, and the publisher logs an error with the message id and event type.

Parked messages stay in the `Outbox` table. There is no replay tool or alert yet. To retry one manually, clear `ParkedAt`, reset `RetryCount` to 0 and clear `NextAttemptOn` in the database.

### Delivery guarantee

Delivery is **at least once**. If a publish succeeds and the database update that sets `ProcessedAt` then fails, the message stays due and is published again. The Worker logs this case and does not count it as a retry. Consumers must tolerate duplicates. Use the `EventId` inside the event content to detect them.

### Shutdown

The Worker stops polling as soon as the host asks it to stop.

- A publish that is cancelled by shutdown is not a failure. The message is left untouched and is picked up on the next start.
- A message that was published just before shutdown is still marked as processed, so it is not published twice.
- A failure that happens during a batch is saved even if shutdown has started.

### LoggingEventBus

`LoggingEventBus` is the only `IEventBus` today. It writes the event type and content to the log at `Information` level and returns. It lets the Worker run with no broker and no Azure credentials.

It is a development fallback, not a message broker. Nothing is stored, delivered or replayable, and a successful publish only means a log line was written. Do not rely on it in production. A real implementation, such as Azure Service Bus, goes behind the same `IEventBus` interface in a later release.

---

## Local setup

### Prerequisites

- .NET 10 SDK
- Docker Desktop

### Run

Create the local database server once. The script needs PowerShell 7 and a running Docker Desktop:

```powershell
git clone <repo-url>
cd bulkza
./scripts/setup-local-sql.ps1
```

The script:

- generates the `sa` password, a password for a limited `bulkza_app` login, and a JWT signing key, using a cryptographic random generator,
- saves the two database passwords in `~/.bulkza/sql.env`, outside the repository and outside any OneDrive folder, readable only by you,
- starts a SQL Server 2025 Developer container named `bulkza-sql`, bound to `127.0.0.1` on port **14330** (not 1433, because a SQL Server installed on the machine usually owns that port),
- creates the `bulkza` database and the `bulkza_app` login, which the Api and Worker use instead of `sa`,
- stores the connection string and JWT key in .NET user secrets for `src/Api` and `src/Worker`,
- applies the EF Core migrations.

It never prints a secret. Running it again is safe and reuses the existing secrets. Then start the apps:

```bash
dotnet restore
dotnet build
dotnet run --project src/Api
```

Run the outbox publisher in a second terminal. It reads the same user secret, and fails at startup with a clear message if the connection string is missing:

```bash
dotnet run --project src/Worker
```

The Worker needs no Azure Service Bus settings. It logs each published event through `LoggingEventBus`. To create the first admin, see [Creating the first admin](#creating-the-first-admin).

### Run everything in Docker

`docker-compose.yml` defines three services: `sqlserver`, `api` and `worker`. After the setup script has created the secrets, build and start the whole stack:

```bash
docker compose --env-file "$HOME/.bulkza/sql.env" up -d --build --wait
```

Run the `docker compose` commands from the repository root, where `docker-compose.yml` is. Write the env file path as `"$HOME/.bulkza/sql.env"`. PowerShell does not expand a bare `~` for programs like `docker`, so `~/.bulkza/sql.env` would be read as a folder named `~` in the current directory.

| Service | Container | Address |
|---------|-----------|---------|
| `sqlserver` | `bulkza-sql` | `127.0.0.1:14330` on the host |
| `api` | `bulkza-api` | `http://127.0.0.1:8080` |
| `worker` | `bulkza-worker` | no port |

- The `Dockerfile` builds both apps. It holds no secrets. Configuration arrives as environment variables from the env file when the container starts.
- The Api and Worker wait for the database to be healthy, reach it by the service name `sqlserver`, and connect as the limited `bulkza_app` login.
- All ports are bound to `127.0.0.1`. Change the Api port with `BULKZA_API_PORT` in the env file.
- To create the first admin, add `ADMIN_SEED_PHONE` and `ADMIN_SEED_PASSWORD` to the env file and recreate the `api` container. Leave them out to skip it.
- The containers restart automatically (`unless-stopped`), including after a reboot.
- Rebuild after code changes with the same command. Follow logs with `docker compose --env-file "$HOME/.bulkza/sql.env" logs -f api worker`.
- Stop everything with `docker compose --env-file "$HOME/.bulkza/sql.env" down`. The database volume is kept. Add `-v` only if you want to delete the data.

The compose file has no default password and refuses to start without `MSSQL_SA_PASSWORD`, `BULKZA_APP_PASSWORD` and `JWT_SECRET`. `.env.example` shows the variable names. Run migrations with the setup script or the `dotnet ef` command below, since neither app applies them on startup.

### Sample products and the web app

The API has no "create product" route yet. Add sample products to the local database with:

```powershell
./scripts/seed-dev-products.ps1
```

The merchant web app lives in `web/`. It runs against the API on port 8080 through a dev proxy:

```bash
cd web
npm install
npm run dev
```

See [web/README.md](web/README.md) for settings, layout and what is not built yet.

### Rotating secrets

```powershell
./scripts/setup-local-sql.ps1 -Rotate
```

This generates new `sa` and app passwords and a new JWT key. It changes the passwords inside SQL Server, rewrites the env file, recreates the SQL Server container and any running Api and Worker containers, and updates the user secrets. Old passwords stop working at once. Old JWTs stop working too, so everyone logs in again.

Rotate whenever a secret may have been seen by someone who should not have it. For a shared or deployed database, rotate the same way and put the new values in your secret store or environment variables, never in a file in the repository.

### Migrations

The Api does not apply migrations on startup. The setup script applies them. To apply them yourself:

```bash
dotnet tool restore
dotnet ef database update --project src/Infrastructure --connection '<connection string>'
```

The design-time factory holds no credentials. Commands that need a database must get one through `--connection` or the `ConnectionStrings__DefaultConnection` environment variable. Commands that only read the model, such as `dotnet ef migrations add`, need no connection.

### Run tests

```bash
dotnet test
```

Api tests start their own SQL Server container, so Docker must be running.

---

## Configuration

Settings load from `appsettings.json`, environment-specific files, and environment variables.

| Key | Used by | Purpose |
|-----|---------|---------|
| `ConnectionStrings:DefaultConnection` | Api, Worker | SQL Server connection string. |
| `Jwt:Secret` | Api | Signing key for tokens. |
| `Jwt:Issuer` | Api | Token issuer. Currently `BulkZa`. |
| `Jwt:Audience` | Api | Token audience. Currently `BulkZaClients`. |
| `Jwt:ExpirationInHours` | Api | Token lifetime. Currently 24. |
| `AdminSeed:Phone` | Api | Phone number of the admin created at startup. Optional. Secret store or environment only. |
| `AdminSeed:Password` | Api | Password of that admin. Optional. Secret store or environment only. |
| `OutboxPublisher:BatchSize` | Worker | Rows per poll. Default 50. Must be at least 1. |
| `OutboxPublisher:PollInterval` | Worker | Wait between polls when nothing was handled. Default 2 seconds. Must be greater than zero. |
| `OutboxPublisher:ErrorDelay` | Worker | Wait after a whole poll fails. Default 5 seconds. |
| `OutboxPublisher:MaxRetryCount` | Worker | Retries after the first attempt before a message is parked. Default 10. Zero parks on the first failure. |
| `OutboxPublisher:InitialRetryDelay` | Worker | Delay before retry 1. Doubles each retry. Default 5 seconds. Must be greater than zero. |
| `OutboxPublisher:MaxRetryDelay` | Worker | Upper bound for the retry delay. Default 5 minutes. Must be at least `InitialRetryDelay`. |

The Worker validates these settings when it starts and refuses to run with invalid values.

### Secrets

The committed `appsettings.json` files hold **no secrets**. `ConnectionStrings:DefaultConnection` and `Jwt:Secret` are empty in the repository and must come from user secrets (set by `scripts/setup-local-sql.ps1`) or from environment variables such as `ConnectionStrings__DefaultConnection` and `Jwt__Secret`.

- Never commit a real connection string, password or signing key, and never paste one into an issue, chat or log.
- The Api and Worker connect as the limited `bulkza_app` login, not `sa`.
- The database port is bound to `127.0.0.1` only.
- The Api tests run in a throwaway container with a random password and a random JWT key per run, so they need no secrets from you.

---

## Testing

- **Domain.Tests**: value objects, `Merchant`, `Product`, `Cart`, `Order`, `BulkPool`, and `VolumePricingStrategy`. Test builders exist for address, bulk pool, cart, merchant, order, and product.
- **Api.Tests**: end-to-end tests through `WebApplicationFactory`. `ApiSmokeTestFixture` starts a SQL Server Testcontainer, applies migrations, and resets data between tests with Respawn. Covers auth endpoints, products, bulk pools, and the admin seeder. Admin-only pool actions run as an admin inserted by `TestDataSeeder.SeedAdminAsync`, and a theory checks that a normal merchant gets 403 on each of them.
- **Worker.Tests**: the outbox publisher. SQL Server tests run the real publisher against a Testcontainers SQL Server (no EF InMemory) with a manual clock. They cover due and future messages, processed messages, raw JSON passthrough, failures, backoff, parking, a failing message inside a batch, and cancellation. Other tests cover `LoggingEventBus`, startup configuration, and option validation. Retry and parking maths are also unit tested in `Domain.Tests` (`OutboxMessageTests`). Docker must be running.
- **Integration.Tests**: JSON round-trip tests for the domain events. **Application.Tests** is a placeholder with no real tests yet.

---

## Roadmap

Not built yet:

- Optimistic concurrency (`rowversion`) on `BulkPool`
- Real Azure Service Bus integration, a billing worker, idempotent consumers, and a dead-letter replay tool (parked outbox rows are the only stand-in today)
- Wiring `VolumePricingStrategy` into pool and order flows, and creating orders when a pool finalises
- Automatic pool expiry and locking
- Structured logging and tracing (Serilog, OpenTelemetry)
- Correlation ID middleware and rate limiting
- CI pipeline (GitHub Actions)
- Infrastructure as code (Terraform) and Azure deployment
- Real payment provider
- Architecture decision records
- Application and integration test coverage
