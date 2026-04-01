# ChatSystem Project Context

A production-grade scalable chat system built with .NET 10.0 and React, following Clean Architecture principles.

## 🚀 Current Status: Phase 3 (Completed)

### **Backend (Distributed Monolith)**
- **Horizontal Scaling:** Implemented **Redis Pub/Sub** for cross-instance communication.
- **SignalR Dispatcher:** Background service synchronizes messages, presence, and typing events across all server instances.
- **Auth:** JWT-based registration and login with BCrypt hashing.
- **Caching:** Redis List-based message caching (last 50 messages/chat).
- **Persistence:** PostgreSQL (EF Core).

### **Frontend (React Client)**
- **Dynamic Configuration:** Supports multiple backend instances via `VITE_API_URL` environment variable.
- **Real-time UI:** Instant updates for messaging, presence, and typing across the cluster.

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
