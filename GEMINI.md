# ChatSystem Project Context

A production-grade scalable chat system built with .NET 10.0 and React, following Clean Architecture principles.

## 🚀 Current Status: Phase 4 (Completed)

### **Backend (Event-Driven with Kafka)**
- **Event-Driven Architecture:** Transitioned from synchronous database writes to an asynchronous, event-driven model using **Apache Kafka**.
- **Service Decoupling:** 
  - **WebSocket Gateway (Hub):** Lightweight ingestion point that publishes raw events to Kafka.
  - **Message Worker:** Background service that consumes `message.send` and persists to PostgreSQL.
  - **Notification Worker:** Background service that consumes `message.stored` and broadcasts to SignalR clients.
- **Reliability & Scaling:**
  - **Idempotence:** Kafka `EnableIdempotence = true` ensures no duplicate messages and strict ordering.
  - **Partitioning:** Messages are partitioned by `chatId`, guaranteeing causal ordering within a chat.
  - **Consumer Groups:** Scalable workers that can be distributed across multiple instances.
- **Redis Integration:** Still used for **Presence Tracking**, **Message Caching**, and **Session Storage**.
- **Testing:** 10 integration tests verified using a custom **MockEventBus loopback**, allowing full flow verification without external infrastructure.

### **Frontend (React Client)**
- **UI:** Modern dark-mode interface (Discord/WhatsApp hybrid).
- **Functionality:** Real-time updates for messages, presence, and typing, now powered by the Kafka-driven backend.

## Project Structure
- `src/ChatSystem.Domain`: Core entities.
- `src/ChatSystem.Application`: Business logic abstractions, DTOs, and **Event Definitions**.
- `src/ChatSystem.Infrastructure`: Persistence, **Kafka Producers/Consumers**, and **Background Workers**.
- `src/ChatSystem.API`: REST endpoints, SignalR Hub, and **Notification Worker**.
- `client/`: React (TypeScript) frontend.

## Building and Running

### 1. Infrastructure (Docker)
```bash
docker compose up -d
```
Starts PostgreSQL, Redis, Kafka, and Zookeeper.

### 2. Database
```bash
dotnet ef database update -p src/ChatSystem.Infrastructure -s src/ChatSystem.API
```

### 3. Start Application
```bash
# Backend
dotnet run --project src/ChatSystem.API

# Frontend
cd client
npm run dev
```

## Key Abstractions
- `IEventProducer`: Publishes events to Kafka topics.
- `IEventConsumer`: Subscribes to and processes event streams.
- `IPresenceService`: Redis-backed global user status.
- `IMessageCache`: Redis-backed recent message storage.
