# MassTransit Order Saga Demo

A working example of an **Order Processing Saga** using:
- [MassTransit](https://masstransit-project.com/)
- RabbitMQ
- Entity Framework Core
- PostgreSQL (with EF Core Migrations)
- Outbox Pattern

---

## 📦 Features
- **Saga State Machine** for managing the order lifecycle
- Event-driven messaging with RabbitMQ
- **Outbox Pattern** for reliable message delivery
- Entity Framework Core repository for saga persistence
- Example consumers for handling stock and payment events
- PostgreSQL schema & migration scripts included

---

## 🗂 Project Structure
src/
├── Consumers/ # Event consumers
├── Messages/ # Message contracts
├── Sagas/ # State machine & state model
├── Program.cs # Application entry point
├── OrdersSagaDbContext.cs
└── ...

---

## 🛠 Requirements
- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download)
- [PostgreSQL](https://www.postgresql.org/)  
- [RabbitMQ](https://www.rabbitmq.com/)  
- Docker (optional, for local RabbitMQ & PostgreSQL)

---

## 🚀 Running Locally

### 1️⃣ Clone the repository
```bash
git clone https://github.com/<your-username>/mass-transit-order-saga.git
cd mass-transit-order-saga
