# ChatSystem Project Context

A production-grade scalable chat system built with .NET 10.0, following Clean Architecture principles. Currently in **Phase 1: Modular Monolith**.

## Project Overview

- **Purpose:** Real-time chat platform supporting authentication, group/private chats, messaging, and real-time notifications.
- **Architecture:** Modular Monolith using Clean Architecture.
  - `src/ChatSystem.Domain`: Core domain entities (User, Chat, Message, ChatMember).
  - `src/ChatSystem.Application`: Business logic abstractions (Interfaces) and Data Transfer Objects (DTOs).
  - `src/ChatSystem.Infrastructure`: External concerns (EF Core DbContext, Authentication, Repositories, Redis implementation).
  - `src/ChatSystem.API`: Entry point, REST Controllers, and SignalR Hubs.
- **Key Technologies:**
  - **Backend:** .NET 10.0, ASP.NET Core Web API, SignalR.
  - **Persistence:** PostgreSQL (Entity Framework Core).
  - **Real-time Backplane:** Redis.
  - **Security:** JWT Authentication, BCrypt password hashing.
  - **Documentation:** Swagger/OpenAPI.

## Building and Running

### Prerequisites
- .NET 10.0 SDK
- Docker Desktop

### Infrastructure Setup
```bash
docker compose up -d
```
Starts PostgreSQL, Redis, and pgAdmin.

### Database Migrations
To use the real PostgreSQL database:
```bash
dotnet ef migrations add InitialCreate -p src/ChatSystem.Infrastructure -s src/ChatSystem.API
dotnet ef database update -p src/ChatSystem.Infrastructure -s src/ChatSystem.API
```

### Running the Application
```bash
dotnet run --project src/ChatSystem.API
```
The API will be available at `http://localhost:5230` (default) with Swagger at `/swagger`.

### Running Tests
```bash
dotnet test
```
Runs integration tests using an in-memory database.

## Development Conventions

- **Clean Architecture:** Strictly maintain separation between layers. Domain should have no dependencies.
- **Dependency Injection:** Use interface-based DI. Register services in `src/ChatSystem.Infrastructure/DependencyInjection.cs` and use them in `src/ChatSystem.API/Program.cs`.
- **Async First:** All I/O-bound operations (DB, Redis, Sockets) must use `async/await`.
- **Real-time:** Use SignalR Hubs (mapped to `/ws`) for real-time events. Authentication for SignalR is supported via the `access_token` query string parameter.
- **Authentication:** REST endpoints are protected with `[Authorize]`. Ensure the `Authorization: Bearer <token>` header is sent.
- **Local Testing:** The application supports an In-Memory database for quick testing without Docker by setting `"UseInMemoryDatabase": "true"` in `appsettings.json`.

## Key Abstractions
- `IAuthService`: Handles registration and login logic.
- `IChatService`: Manages chat creation, membership, and message history.
- `IUserRepository`, `IChatRepository`, `IMessageRepository`: Data access abstractions.
- `IJwtProvider`: Centralized JWT generation.
