# Chat-System

A production-grade scalable chat system built with .NET 10.0 and React, following Clean Architecture principles.

## 🚀 Project Status

### Phase 1: Modular Monolith (Completed)
- **Authentication:** JWT-based registration and login with BCrypt hashing.
- **Messaging:** CRUD operations for chats and messages via REST API.
- **Real-Time:** Persistent real-time message broadcasting, presence tracking, and typing indicators using SignalR.
- **Persistence:** PostgreSQL with Entity Framework Core.
- **Infrastructure:** Docker-ready environment (PostgreSQL, Redis, pgAdmin).

### Phase 1.5: React Frontend (Completed)
- **Modern UI:** Hybrid Discord/WhatsApp design with dark mode.
- **Features:** 
  - Real-time chat with message history.
  - Presence list and typing indicators.
  - Chat management (Create group/private chats, add members).
  - User search with debounced queries.

## 🏗 Architecture
The backend follows **Clean Architecture**:
- **Domain:** Core entities (`User`, `Chat`, `Message`) and business logic.
- **Application:** Interfaces, DTOs, and application logic.
- **Infrastructure:** Persistence, authentication, connection tracking, and external service implementations.
- **API:** Controllers, SignalR Hubs, and application entry point.

The frontend is a **React (TypeScript)** SPA using Vite and Context API for state management.

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
API: `http://localhost:5230` | Swagger: `/swagger`

### 4. Run Frontend
```bash
cd client
npm install
npm run dev
```
Frontend: `http://localhost:5173`

## 🧪 Testing
Run automated integration tests:
```bash
dotnet test
```

## ⏩ Next Phases
- **Phase 2:** Distributed Backplane (Redis), Service Decomposition, and Kafka Integration.
- **Phase 3:** Fully distributed microservices architecture.
