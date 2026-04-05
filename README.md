# Chat-System

A production-grade scalable chat system built with .NET 10.0 and React, following Clean Architecture principles.

## 🚀 Project Status

### Phase 1 & 1.5: Core System & UI (Completed)
- **Authentication:** JWT-based registration and login with BCrypt hashing.
- **Messaging:** CRUD operations for chats and messages via REST API.
- **Real-Time UI:** Hybrid Discord/WhatsApp design with persistent real-time messaging, presence tracking, and typing indicators using SignalR.
- **Modern UI:** Hybrid Discord/WhatsApp design with dark mode.
- **Features:** 
  - Real-time chat with message history.
  - Presence list and typing indicators.
  - Chat management (Create group/private chats, add members).
  - User search with debounced queries.

### Phase 2: Distributed Caching (Completed)
- **Redis Presence:** User online status and "last seen" tracking stored in Redis.
- **Message Cache:** Cache-aside pattern using Redis Lists for the last 50 messages per chat.
- **Fail-safe:** Graceful fallback to PostgreSQL if Redis is offline.

### Phase 3: Horizontal Scaling (Completed)
- **Distributed Messaging:** **Redis Pub/Sub** backplane synchronizes SignalR events across multiple server instances.
- **SignalR Dispatcher:** Background service ensures messages reach users regardless of which server they are connected to.

## 🏗 Architecture
The system follows **Clean Architecture** and has evolved into a **Distributed Monolith**:
- **Domain:** Core entities and business logic.
- **Application:** Interfaces, DTOs, and application logic.
- **Infrastructure:** Persistence (PostgreSQL), Distributed State (Redis), and Pub/Sub.
- **API Nodes:** Multiple stateless API instances behind a load balancer (simulated).

## 🛠 Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js (v18+)](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Entity Framework Core CLI Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

## 🚦 Getting Started

### 1. Setup Infrastructure
Run Docker Compose to start PostgreSQL and Redis:
```bash
docker compose up -d
```

### 2. Apply Migrations
```bash
dotnet ef database update -p src/ChatSystem.Infrastructure -s src/ChatSystem.API
```

### 3. Run Backend
```bash
dotnet run --project src/ChatSystem.API
```

### 4. Run Frontend
```bash
cd client
npm install
npm run dev
```

### 5. Multi-Instance Scaling Test
To test horizontal scaling, start two instances:
```bash
# Terminal 1 (Server 1)
dotnet run --project src/ChatSystem.API --urls="http://localhost:5230"

# Terminal 2 (Server 2)
dotnet run --project src/ChatSystem.API --urls="http://localhost:5231"

# Terminal 3 (Frontend 1 -> Server 1)
cd client
npm run dev -- --port 5173

# Terminal 4 (Frontend 2 -> Server 2)
cd client
$env:VITE_API_URL="http://localhost:5231"; npm run dev -- --port 5174
```
Users on different ports can now chat in real-time via the Redis backplane!

## 🧪 Testing
Run automated integration tests:
```bash
dotnet test
```

## ⏩ Next Phases
- **Phase 4:** Event-Driven Architecture with **Apache Kafka**.
- **Phase 5:** Microservices Decomposition.