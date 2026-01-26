# PremiumPlace

PremiumPlace is an API-first, full-stack booking platform inspired by the Airbnb concept: hosts can publish places, guests can browse place details, and bookings/reviews are supported (or planned) as part of the product roadmap.

The mono-repo is centered around a **stateless ASP.NET Core REST API** that serves multiple clients (ASP.NET Core MVC and an Angular SPA) via a shared DTO contract.

---

## Repository structure

- **`PremiumPlaceApi/`** → **ASP.NET Core REST API (main project)**
- **`PremiumPlace.DTO/`** → Shared DTOs used by API and clients
- **`PremiumPlace_Web/`** → ASP.NET Core MVC client consuming the API
- **`premiumplace-angular/`** → Angular frontend (SPA) consuming the API (under active development)

---

## Architecture overview

### API-first design

PremiumPlace is designed around an API-first approach where the backend exposes a well-defined REST surface that is the primary integration point for all user experiences.

### Multi-client ecosystem

The same API is consumed by:

- a server-rendered **ASP.NET Core MVC** client (`PremiumPlace_Web`)
- a **single-page Angular** application (`premiumplace-angular`)

This structure encourages consistent business rules, shared validation and data shapes (via DTOs), and independent evolution of UI clients.

### Stateless REST API

The API is implemented as a stateless HTTP service:

- requests are authenticated/authorized per call
- state is persisted in the database (not in server memory)
- clients can be deployed and scaled independently of the API

### Separation of concerns

The solution separates responsibilities across:

- API transport layer (controllers / HTTP concerns)
- business logic (services / use-case-oriented code)
- persistence (EF Core / database concerns)
- contracts (DTOs shared across clients)

---

## PremiumPlace API (ASP.NET Core REST API)

`PremiumPlaceApi` is the core of the system and the primary entry point for domain operations such as place management and bookings.

### Technology stack

- **ASP.NET Core** (REST API)
- **Entity Framework Core** (data access)
- **SQL Server** (persistence)
- **JWT-based authentication**
- **HttpOnly cookies** (session/token storage approach where applicable)

> Note: The exact hosting and production topology is intentionally not assumed here; the API is documented to be locally runnable and evolvable toward production deployment.

### Authentication & authorization

The API follows a modern authentication setup:

- **JWT tokens** represent authenticated user sessions
- tokens may be delivered/stored using **HttpOnly cookies** to reduce exposure to XSS
- authorization is enforced at the endpoint level (e.g., requiring authentication for write operations)

Typical authorization expectations:

- unauthenticated users can browse/search places
- authenticated users can create/manage their own places and bookings
- administrative or elevated actions can be introduced via roles/claims as the platform grows

### Main API capabilities

#### Places management

Core operations for hosts and administrators:

- create a new place listing
- update listing data (title, description, pricing, location, etc.)
- manage listing visibility / lifecycle (as applicable)

#### Place details

Read-oriented endpoints designed for discovery and conversion:

- list places (with filtering/paging as needed)
- retrieve a place by id/slug
- retrieve enriched details (images, amenities, host info, etc., depending on implementation)

#### Booking (conceptual / evolving)

Booking flows are modeled as first-class API operations, even if partially implemented today:

- request a booking for a date range
- validate booking rules (ownership, date validity, conflicts)
- store booking records and status transitions (e.g., pending/confirmed/cancelled)

#### Reviews (conceptual / evolving)

A review system typically includes:

- create a review for a completed stay
- list reviews per place
- compute derived metrics (e.g., average rating)

### API design principles

The API is implemented with maintainability and client interoperability in mind:

- **Clean layering**: HTTP concerns stay at the edge; business logic lives in services; persistence is isolated behind EF Core.
- **DTO-first contracts**: request/response models are expressed via DTOs in `PremiumPlace.DTO` to keep clients aligned and reduce accidental coupling to persistence entities.
- **Mapping**: controlled mapping between persistence entities and DTOs keeps the public contract stable while allowing internal refactoring.
- **Service-oriented use cases**: operations are expressed in terms of business actions (e.g., “create place”, “book place”) rather than direct CRUD-only data access.

### Example endpoints (illustrative)

The following is a conceptual overview of the REST surface (not a full Swagger dump):

- **Places**
  - `GET /api/places` — List places (optionally filtered/paged)
  - `GET /api/places/{id}` — Get place details
  - `POST /api/places` — Create a place (authenticated)
  - `PUT /api/places/{id}` — Update a place (authenticated, owner-only)

- **Bookings**
  - `POST /api/bookings` — Create a booking request (authenticated)
  - `GET /api/bookings/me` — List current user bookings (authenticated)

- **Auth**
  - `POST /api/auth/register` — Register user
  - `POST /api/auth/login` — Authenticate user (JWT / cookie-based session approach)
  - `POST /api/auth/logout` — Clear session (where applicable)

---

## Other projects

### PremiumPlace.DTO

Shared contract library containing DTOs used by:

- `PremiumPlaceApi` for request/response bodies
- UI clients for strongly-typed integration

This helps keep API integration explicit and reduces drift between backend and frontend models.

### PremiumPlace_Web (ASP.NET Core MVC client)

A server-rendered MVC client that consumes the REST API. It is useful for:

- validating API completeness from a traditional web UI perspective
- providing an alternative UI surface that can evolve independently of the SPA

### premiumplace-angular (Angular SPA)

A single-page application that consumes the same REST API.

Status:

- **under active development**
- expected to become the primary interactive client once finalized

---

## Future plans

Planned enhancements focus on completing the booking platform experience end-to-end:

- **Booking system** with robust availability checks and conflict prevention
- **Calendar integration** (host availability management and guest selection UX)
- **Payments** (payment intent creation, capture/confirmation, and refund workflows)
- **Production deployment** (environment-based configuration, CI/CD, observability, and hardened security posture)

---

## Live demo

Live demo links will be added once the Angular frontend is finalized.
