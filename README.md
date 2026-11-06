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

tests/
    Domain.Tests/
    Api.Tests/
    Application.Tests/     (placeholder)
    Integration.Tests/     (placeholder)
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
| `products` | `GET browse`, `GET search/name`, `GET search/sku` |
| `carts` | `GET`, `POST items`, `PUT items/{productId}`, `DELETE items/{productId}`, `DELETE clear` |
| `bulkpools` | `POST`, `GET mine`, `POST {id}/join`, `POST {id}/leave`, `POST {id}/lock`, `POST {id}/finalise`, `POST {id}/expire`, `POST {id}/cancel` |
| `orders` | `GET mine`, `POST {id}/pay`, `POST {id}/cancel`, `POST {id}/ship`, `POST {id}/deliver` |

Conventions:

- Controllers use request and response DTOs, not domain entities.
- Errors return RFC 7807 `ProblemDetails` through `GlobalExceptionHandler`.
- The merchant id comes from JWT claims, not the request body.
- Exception mapping: `ArgumentException` and `InvalidOperationException` return 400, `UnauthorizedAccessException` returns 401, `KeyNotFoundException` returns 404, anything else returns 500.

Middleware order: exception handler, authentication, authorization, controllers.

Application handlers exist for merchant updates (name, phone number, address, status), product management (create, activate, deactivate, update price, category, description, unit of measure), and order creation. Not every handler has a controller route yet.

Payments go through `IPaymentGateway`. The only implementation is `MockPaymentGateway`, which always succeeds.

---

## Transactional outbox

`BulkzaDbContext.SaveChangesAsync` collects the domain events from every tracked `BulkPool`. It writes them as `OutboxMessage` rows in the same save, then clears the events from the aggregate.

```
Handler changes a BulkPool
        ↓
SaveChangesAsync writes the pool and the outbox rows in one transaction
        ↓
OutboxPublisher (Worker) polls for unprocessed rows
        ↓
IEventBus.PublishAsync
        ↓
Row marked as processed
```

`OutboxMessage` fields: `Id`, `Type` (assembly-qualified type name), `Content` (JSON), `OccuredAt`, `ProcessedAt`, `Error`, `RetryCount`, `NextAttemptOn`.

Publisher behaviour:

- Polls for rows where `ProcessedAt` is null and `NextAttemptOn <= now`, oldest first, up to `BatchSize` rows.
- Sleeps for `PollInterval` when there is nothing to publish.
- On a publish failure, calls `RecordFailure`, then waits `ErrorDelay`.

Retry logic in `OutboxMessage.RecordFailure`:

- Each failure increments `RetryCount` and sets `NextAttemptOn` using exponential backoff: `5 * 2^RetryCount` seconds, capped at 60. The delays are 10, 20, 40, then 60 seconds for every later attempt.
- The retry limit is 20. Once `RetryCount` reaches 20, the next failure sets `NextAttemptOn` to `DateTimeOffset.MaxValue` and returns. The publisher never selects the row again, so it is parked and needs manual attention.
- Parked rows stay in the `Outbox` table with `ProcessedAt` null. There is no dead-letter queue or alerting yet.
- `RecordFailure` does not store the error text, so `Error` stays null.

The Worker registers `InMemoryEventBus` as `IEventBus`. This is a development stand-in. Events are not delivered to an external broker.

---

## Local setup

### Prerequisites

- .NET 10 SDK
- Docker Desktop

### Run

```bash
git clone <repo-url>
cd bulkza
export MSSQL_SA_PASSWORD='<choose a strong password>'
docker compose up -d
dotnet restore
dotnet build
dotnet run --project src/Api
```

Run the outbox publisher in a second terminal:

```bash
dotnet run --project src/Worker
```

`docker-compose.yml` starts a single SQL Server 2025 Developer container named `bulkza` on port 1433, with a named volume and a health check. It reads the SA password from `MSSQL_SA_PASSWORD`.

The Api does not apply migrations on startup. Apply them before the first run:

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

The `dotnet-ef` tool is pinned in `dotnet-tools.json`. Run `dotnet tool restore` first.

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
| `OutboxPublisher:BatchSize` | Worker | Rows per poll. Default 50. |
| `OutboxPublisher:PollInterval` | Worker | Idle wait. Default 2 seconds. |
| `OutboxPublisher:ErrorDelay` | Worker | Wait after a failure. Default 5 seconds. |

Do not commit real connection strings or signing keys. Use user secrets or environment variables for local values, for example `ConnectionStrings__DefaultConnection` and `Jwt__Secret`.

---

## Testing

- **Domain.Tests**: value objects, `Merchant`, `Product`, `Cart`, `Order`, `BulkPool`, and `VolumePricingStrategy`. Test builders exist for address, bulk pool, cart, merchant, order, and product.
- **Api.Tests**: end-to-end tests through `WebApplicationFactory`. `ApiSmokeTestFixture` starts a SQL Server Testcontainer, applies migrations, and resets data between tests with Respawn. Covers auth endpoints, products, and bulk pools.
- **Application.Tests** and **Integration.Tests**: placeholder projects with no real tests yet.

---

## Roadmap

Not built yet:

- Optimistic concurrency (`rowversion`) on `BulkPool`
- Real Azure Service Bus integration, a billing worker, idempotent consumers, and a dead-letter queue (parked outbox rows are the only stand-in today)
- Wiring `VolumePricingStrategy` into pool and order flows, and creating orders when a pool finalises
- Automatic pool expiry and locking
- Structured logging and tracing (Serilog, OpenTelemetry)
- Correlation ID middleware and rate limiting
- CI pipeline (GitHub Actions)
- Infrastructure as code (Terraform) and Azure deployment
- Real payment provider
- Architecture decision records
- Application and integration test coverage
