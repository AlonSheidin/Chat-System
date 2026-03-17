# ChatSystem Project Context

A production-grade scalable chat system built with .NET 10.0 and React, following Clean Architecture principles.

## 🚀 Current Status: Phase 1 & 1.5 (Completed)

### **Backend (Modular Monolith)**
- **Auth:** JWT-based registration and login with BCrypt hashing.
- **User Management:** Search users, profile retrieval.
- **Messaging:** CRUD for chats and messages.
- **Real-time:** SignalR hub for persistent real-time messaging, presence tracking, and typing indicators.
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
- `src/ChatSystem.Infrastructure`: Persistence, SignalR connection tracking, and external services.
- `src/ChatSystem.API`: REST endpoints and SignalR Hub.
- `client/`: React (TypeScript) frontend.

## Building and Running

### 1. Infrastructure (Docker)
```bash
docker compose up -d
```

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
- **State Management:** React Context for Auth and Chat states.
- **Throttling:** Client-side debouncing for user search and typing indicators.

## Key Abstractions
- `IAuthService`, `IUserService`, `IChatService`: Application logic.
- `IConnectionTracker`: Singleton managing socket-to-user mapping.
- `ChatContext`: Unified frontend state for real-time data.
