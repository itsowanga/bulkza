# BulkZA

A bulk-buying platform where multiple merchants combine their orders to unlock volume discounts, pay individually, and receive goods grouped for delivery.

---

## Table of contents

1. [Problem](#problem)
2. [Solution](#solution)
3. [Architecture](#architecture)
4. [Core patterns](#core-patterns)
5. [Technologies](#technologies)
6. [Project structure](#project-structure)
7. [Domain model](#domain-model)
8. [BulkPool state machine](#bulkpool-state-machine)
9. [Pricing](#pricing)
10. [API](#api)
11. [Messaging](#messaging)
12. [Local setup](#local-setup)
13. [Configuration](#configuration)
14. [Testing](#testing)
15. [CI](#ci)
16. [Azure deployment](#azure-deployment)
17. [Threat model](#threat-model)
18. [ADRs](#adrs)
19. [Scope](#scope)

---

## Problem

Small merchants in South Africa often can't reach the order volumes needed to qualify for wholesale pricing. BulkZA lets them pool their demand: when enough merchants commit to a product, the group crosses a pricing threshold and everyone benefits from the lower tier.

## Solution

- Merchants browse a product catalog and add items to a cart.
- Merchants join or create **bulk pools** for a specific product.
- When a pool reaches its minimum quantity and merchant count, it locks and finalizes.
- Each merchant receives an individual **order** priced at the achieved volume tier.
- Payments are collected per merchant; fulfillment follows.

---

## Architecture

The solution follows a layered architecture with dependencies pointing inward:

```
Api / Worker
     ↓
Application
     ↓
Domain

Infrastructure
     ↓
Application
     ↓
Domain
```

| Layer | Responsibility |
|-------|----------------|
| **Domain** | Entities, value objects, aggregates, `BulkPool` state machine, pricing rules. No infrastructure dependencies. |
| **Application** | Use cases, orchestration, interfaces (`IPricingStrategy`, `IPaymentGateway`, repository abstractions). |
| **Infrastructure** | EF Core + SQL Server, Service Bus, outbox publisher, mocked payment gateway. |
| **Api** | ASP.NET Core Web API (auth, merchants, products, carts, pools, orders). |
| **Worker** | Background consumers (billing worker with idempotency, retries, dead-letter handling). |

---

## Core patterns

- **Transactional Outbox** — domain events written atomically with business data, then published to Service Bus by a background publisher.
- **Idempotent consumers** — duplicate Service Bus messages are ignored via a `ProcessedMessage` table.
- **Optimistic concurrency** — `rowversion` on `BulkPool` prevents race conditions during concurrent joins.
- **State machine** — `Draft → Open → ThresholdMet → Locked → Finalized`, with `Expired` and `Cancelled` as terminal alternatives.

---

## Technologies

- .NET 10 (C# latest)
- ASP.NET Core Web API
- Entity Framework Core 10 + SQL Server
- Azure Service Bus
- Serilog + OpenTelemetry
- Docker / Docker Compose
- xUnit + FluentAssertions + Testcontainers
- GitHub Actions (build + test)
- Terraform (Azure infrastructure)

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
    Application.Tests/
    Integration.Tests/
    Api.Tests/
```

---

## Domain model

### Value objects

- `MerchantId`, `ProductId`, `PoolId`, `OrderId`, `CartId`
- `Address`, `PhoneNumber`
- `Money`, `Quantity`

Value objects are immutable, validate their own invariants, and use value equality.

### Entities / aggregates

- `Merchant`
- `Product`
- `BulkPool` (aggregate root)
- `Cart` + `CartItem`
- `Order` + `OrderLine`

### Domain events

- `MerchantJoinedPool`
- `MerchantLeftPool`
- `PoolThresholdReached`
- `PoolLocked`
- `PoolFinalized`
- `PoolExpired`

---

## BulkPool state machine

```
Draft
  ↓
Open
  ↓
ThresholdMet
  ↓
Locked
  ↓
Finalized
```

Terminal alternatives:

```
Expired
Cancelled
```

Illegal transitions (examples):

- `Finalized → Open`
- `Expired → Open`
- `Cancelled → Join`
- `Locked → AddQuantity`

State changes go through domain methods (`Join`, `Leave`, `Lock`, `Finalize`, `Expire`), never by setting a status property.

---

## Pricing

One strategy for v1, behind an interface so more can be added later.

```
IPricingStrategy
    ↓
VolumePricingStrategy
```

Example tiers:

| Quantity   | Unit price |
|------------|-----------|
| 1–9        | R100      |
| 10–49      | R95       |
| 50–99      | R90       |
| 100+       | R85       |

Rules:

- Use `decimal` for all money.
- Round with `MidpointRounding` explicitly.
- Currency is ZAR.
- Pricing is deterministic and pure.
- Test every boundary: 0, 1, 9, 10, 49, 50, 99, 100, and invalid/large quantities.

---

## API

Base groups:

```
/merchants
/products
/carts
/pools
/pools/{id}/join
/pools/{id}/leave
/orders
/payments
```

Conventions:

- Request and response DTOs (never expose domain entities).
- RFC 7807 `ProblemDetails` for all errors.
- Consistent status codes: 200, 201, 400, 401, 403, 404, 409, 500.
- Auth via JWT bearer tokens.
- Merchant identity comes from claims, never from the request body.

Middleware order:

1. Exception handling
2. Correlation ID
3. Authentication
4. Authorization
5. Request logging

---

## Messaging

Flow:

```
Business transaction
      ↓
Database changes + Outbox event
      ↓
COMMIT
      ↓
Background publisher (Worker)
      ↓
Azure Service Bus
      ↓
Billing worker (idempotent)
```

Outbox message fields:

- `Id`
- `Type`
- `Payload`
- `OccurredAt`
- `ProcessedAt`
- `Attempts`
- `Error`

Consumer idempotency:

- Check `MessageId` against `ProcessedMessage`.
- If already processed → ignore.
- Otherwise process, save `MessageId` in the same transaction.
- Never assume exactly-once delivery; design for at-least-once.

Retries and DLQ:

- Exponential backoff for transient failures.
- Max retry attempts, then dead-letter.
- Do not retry permanent failures.

---

## Local setup

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- SQL Server (via Docker)
- Azure Service Bus namespace (or emulator where supported)

### Run locally

```bash
git clone <repo-url>
cd BulkZA
dotnet restore
dotnet build
docker compose up -d
dotnet run --project src/Api
```

### Run tests

```bash
dotnet test
```

Integration tests use Testcontainers and start their own SQL Server instance.

---

## Configuration

Loaded from `appsettings.json`, environment variables, and (locally) user secrets.

Required settings — never commit real values:

```
ConnectionStrings:SqlServer
ServiceBus:ConnectionString
Jwt:Issuer
Jwt:Audience
Jwt:SigningKey
```

---

## Testing

- **Unit tests** — value objects, entities, aggregates, state machine, pricing.
- **Integration tests** — API endpoints, auth, persistence, transactions, concurrency, outbox, message processing.
- **Test builders** — `MerchantBuilder`, `ProductBuilder`, `PoolBuilder`, `OrderBuilder`.
- **Testcontainers** — real SQL Server instance per test run.
- **Concurrency test** — simultaneous pool joins, verify final quantity and no duplicates.
- **Messaging tests** — success, duplicate, retry, failure, dead-letter.

---

## CI

GitHub Actions runs on every push:

1. Restore
2. Build
3. Unit tests
4. Integration tests

Out of scope for v1 CI: security scanning, Docker build/publish, vulnerability gates.

---

## Azure deployment

Infrastructure provisioned with **Terraform**:

- App Service or Container App (API)
- Azure SQL
- Azure Service Bus
- Supporting resources (Key Vault, Application Insights)

Secrets are never hard-coded — use Key Vault and managed identity.

---

## Threat model

| Threat | Mitigation |
|--------|-----------|
| IDOR (accessing another merchant's data) | Derive merchant identity from JWT claims; authorize every resource access. |
| Injection | Parameterized queries via EF Core; validate all input. |
| Credential theft | Password hashing; no plaintext storage. |
| Token theft | Short-lived JWTs; HTTPS only. |
| Replay | Idempotency keys on payments; `ProcessedMessage` on consumers. |
| Message tampering | Service Bus auth; validate payloads on receipt. |
| Duplicate messages | Idempotent consumers. |
| Secret leakage | User secrets locally; Key Vault in Azure; never log secrets. |
| Excessive requests | Rate limiting at the API edge. |

---

## ADRs

| ADR | Topic |
|-----|-------|
| ADR-001 | Domain architecture |
| ADR-002 | BulkPool state machine |
| ADR-003 | Pricing strategy pattern |
| ADR-004 | SQL isolation level |
| ADR-005 | Optimistic concurrency |
| ADR-006 | Transactional Outbox |
| ADR-007 | Service Bus |
| ADR-008 | Terraform vs Bicep |
| ADR-009 | Result vs exceptions |

Each ADR contains: Context, Decision, Alternatives, Consequences.

---

## Scope

### In scope for v1

- Domain: `BulkPool` state machine, one pricing strategy, core aggregates and value objects
- API: auth, merchants, products, carts, bulk pools, orders
- SQL Server + EF Core as transactional source of truth, optimistic concurrency on `BulkPool`, transactions on critical operations
- Transactional Outbox + background publisher
- One Service Bus worker (billing), idempotent, with retries and DLQ
- Mocked payment gateway
- Serilog + OpenTelemetry across API, outbox, and the worker
- Docker Compose (SQL Server, app containers)
- GitHub Actions: build + test only
- Terraform: core Azure resources (App Service/Container App, SQL, Service Bus)

### Deferred to v2

- Cosmos DB geospatial clustering
- SMS mock
- Additional workers (logistics, notifications)
- App Insights dashboards and alerting
- Security scanning and packaging in CI
- Driver / Wholesaler / Admin experiences
- Real payment and SMS providers
- Full ASP.NET Identity, OAuth/OIDC, 2FA
- Kubernetes, AKS, Helm
- CQRS, MediatR-heavy architecture, event sourcing, saga
- Kafka, gRPC, GraphQL, SignalR
- Multi-region, RLS, complex accounting
