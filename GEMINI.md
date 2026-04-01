# ChatSystem Project Context

A production-grade scalable chat system built with .NET 10.0 and React, following Clean Architecture principles.

## 🚀 Current Status: Phase 2 (Completed)

### **Backend (Modular Monolith + Redis)**
- **Auth:** JWT-based registration and login with BCrypt hashing.
- **User Management:** Search users, profile retrieval enriched with **Real-time Status and Last Seen** info.
- **Messaging:** 
  - CRUD for chats and messages.
  - **Message Caching:** Implemented Cache-Aside pattern using **Redis Lists**. Caches last 50 messages per chat with a 10-minute TTL.
- **Real-time:** SignalR hub for messaging, presence tracking, and typing indicators.
- **Redis Integration:**
  - **Presence:** Global online/offline tracking stored in Redis.
  - **Fail-safe:** System automatically falls back to PostgreSQL if Redis is unavailable.
- **Persistence:** PostgreSQL (EF Core).
- **Testing:** 10 integration tests verifying core flows.

### **Frontend (React Client)**
- **UI:** Modern dark-mode interface (Discord/WhatsApp hybrid).
- **Functionality:** 
  - Integrated with SignalR for instant updates.
  - Chat management (Create chats, add members).
  - Global presence and typing feedback.

## Project Structure
- `src/ChatSystem.Domain`: Core entities.
- `src/ChatSystem.Application`: Business logic abstractions and DTOs.
- `src/ChatSystem.Infrastructure`: Persistence, SignalR connection tracking, **Redis Services**, and caching logic.
- `src/ChatSystem.API`: REST endpoints and SignalR Hub.
- `client/`: React (TypeScript) frontend.

## Building and Running

### 1. Infrastructure (Docker)
```bash
docker compose up -d
```
*Note: Ensure Docker is running to start PostgreSQL and Redis.*

### 2. Database
```bash
dotnet ef database update -p src/ChatSystem.Infrastructure -s src/ChatSystem.API
```

### 3. Start Backend
```bash
dotnet run --project src/ChatSystem.API
```

### 4. Start Frontend
```bash
cd client
npm run dev
```

## Development Conventions
- **Clean Architecture:** Strictly separate layers.
- **Async First:** All I/O is asynchronous.
- **Caching:** Use `IMessageCache` for read-heavy operations.
- **Fail-safe:** Always wrap external cache calls in try-catch to ensure availability.

## Key Abstractions
- `IRedisService`: Low-level Redis operations.
- `IPresenceService`: User status and last seen management.
- `IMessageCache`: Recent message storage.
- `IConnectionTracker`: Local socket-to-user mapping.
