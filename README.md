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
```
2️⃣ Set up PostgreSQL & RabbitMQ (via Docker)

docker-compose up -d
(docker-compose.yml is included in this repo)

3️⃣ Apply EF Core migrations

dotnet ef database update
4️⃣ Run the app

dotnet run
🔄 Workflow
OrderSubmitted event starts the saga.

Saga requests stock reservation.

If stock is available, saga requests payment authorization.

Once payment is authorized, saga completes the order.

Saga is marked as completed and archived.

🧪 Testing
You can send events using any message publisher (MassTransit client, Postman with RabbitMQ HTTP API, etc.).

Example payload for OrderSubmitted:

{
  "OrderId": "d290f1ee-6c54-4b01-90e6-d701748f0851",
  "CustomerEmail": "customer@example.com",
  "Total": 99.99
}
