# ChatSystem Project Context

A production-grade scalable chat system built with .NET 10.0, following Clean Architecture principles. Currently, **Phase 1: Modular Monolith** is fully complete and verified.

## Project Overview

- **Purpose:** Real-time chat platform supporting authentication, group/private chats, messaging, and real-time notifications.
- **Architecture:** Modular Monolith using Clean Architecture.
  - `src/ChatSystem.Domain`: Core domain entities (User, Chat, Message, ChatMember).
  - `src/ChatSystem.Application`: Business logic abstractions (Interfaces) and Data Transfer Objects (DTOs).
  - `src/ChatSystem.Infrastructure`: External concerns (EF Core DbContext, Authentication, Repositories, Connection Tracking, Redis preparation).
  - `src/ChatSystem.API`: Entry point, REST Controllers, and SignalR Hubs.

## 🚀 Status: Phase 1 (Completed)
- **Authentication:** JWT-based registration and login with BCrypt hashing.
- **User Management:** Search users by username/email, profile retrieval.
- **Messaging:** 
  - REST API for chat management and message history.
  - **Unified Real-time:** Messages sent via SignalR are validated, persisted to PostgreSQL, and broadcasted.
- **Real-time Features:**
  - **Presence:** Global online/offline tracking with synchronization for new connections.
  - **Typing Indicators:** Visual feedback when users are typing in specific chats.
- **Testing:** 10 comprehensive integration tests covering Auth, Chats, Users, and SignalR Hub logic.

## Building and Running

### Infrastructure Setup
```bash
docker compose up -d
```
Starts PostgreSQL, Redis, and pgAdmin.

### Database Migrations
```bash
dotnet ef database update -p src/ChatSystem.Infrastructure -s src/ChatSystem.API
```

### Running the Application
```bash
dotnet run --project src/ChatSystem.API
```
API: `http://localhost:5230` | Swagger: `/swagger` | WebSockets: `/ws`

### Manual Testing
Open `tests/ChatSystem.ManualTests/index.html` in multiple browser windows to test real-time presence, typing, and messaging.

## Development Conventions
- **Clean Architecture:** Domain has zero dependencies. Logic is interface-driven.
- **Async First:** All I/O operations are asynchronous.
- **Thread Safety:** Singleton services (like `ConnectionTracker`) use thread-safe collections and locking for nested data.
- **Local Testing:** Supports `"UseInMemoryDatabase": "true"` in `appsettings.json` for quick verification.

## Key Abstractions
- `IAuthService`, `IUserService`, `IChatService`: Core business logic.
- `IUserRepository`, `IChatRepository`, `IMessageRepository`: Data persistence.
- `IConnectionTracker`: Manages user-to-socket mapping for presence.
- `IJwtProvider`: Centralized security token generation.
