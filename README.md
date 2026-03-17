# Chat-System

A production-grade scalable chat system built with .NET 10.0 following Clean Architecture principles.

## 🚀 Status: Phase 1 (Modular Monolith)
Currently, the system is a fully functional modular monolith, providing:
- **Authentication:** JWT-based user registration and login with BCrypt password hashing.
- **Messaging:** CRUD operations for chats and messages via REST API.
- **Real-Time:** Real-time message broadcasting using SignalR (WebSockets).
- **Persistence:** PostgreSQL with Entity Framework Core.
- **Infrastructure:** Docker-ready environment including PostgreSQL, Redis, and pgAdmin.

## 🏗 Architecture
The project follows **Clean Architecture**:
- **Domain:** Core entities (`User`, `Chat`, `Message`) and business logic.
- **Application:** Interfaces, DTOs, and application logic.
- **Infrastructure:** Persistence, authentication provider, and external service implementations.
- **API:** Controllers, SignalR Hubs, and application entry point.

## 🛠 Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Entity Framework Core CLI Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

## 🚦 Getting Started

### 1. Setup Configuration
Rename `src/ChatSystem.API/appsettings.Example.json` to `appsettings.json` and update the database credentials.

### 2. Start Infrastructure
Run Docker Compose to start PostgreSQL and Redis:
```bash
docker compose up -d
```

### 3. Run Migrations
Apply the database schema:
```bash
dotnet ef database update -p src/ChatSystem.Infrastructure -s src/ChatSystem.API
```

### 4. Start the Application
```bash
dotnet run --project src/ChatSystem.API
```
The API will be available at `http://localhost:5230` with Swagger documentation at `/swagger`.

## 🧪 Testing
Run automated integration tests:
```bash
dotnet test
```
*Note: Tests use an in-memory database and do not require Docker.*

## ⏩ Next Phases
- **Phase 2:** Distributed Backplane (Redis), Service Decomposition, and Kafka Integration.
- **Phase 3:** Fully distributed microservices architecture.
