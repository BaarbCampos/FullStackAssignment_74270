# 🚀 SportsStore – Distributed Order Processing Platform

## 📌 Project Overview

SportsStore is a distributed e-commerce order processing system built using **.NET microservices** and **RabbitMQ** for asynchronous communication.

The platform simulates the full lifecycle of a customer order, from checkout to completion, using an **event-driven architecture**. Instead of processing everything inside one application, the workflow is divided into multiple independent services that communicate through messages.

This project demonstrates how modern distributed systems handle order processing using:

- Event-driven architecture
- Decoupled services
- Asynchronous communication
- Microservice-based design

---

## 🧩 Technologies Used

### 🔹 Backend
- **.NET / ASP.NET Core Web API**
- **C#**
- **RabbitMQ**
- **Entity Framework Core**
- **SQLite**
- **Serilog**
- **AutoMapper**

### 🔹 Frontend
- **React (Vite)** – Admin Dashboard
- **Blazor / Razor Pages** – Customer Portal

### 🔹 DevOps / Deployment
- **Docker**
- **Docker Compose**

---

## 🏗️ System Architecture

The solution is organized into multiple services, each responsible for a specific step in the order processing workflow.

### 📦 Solution Structure

#### **SportsStore.OrderApi**
- Central entry point of the system
- Exposes REST API endpoints
- Creates and stores orders
- Publishes events to RabbitMQ
- Tracks order status updates

#### **SportsStore.InventoryService**
- Consumes order events
- Checks stock availability
- Publishes:
  - `InventoryConfirmed`
  - `InventoryFailed`

#### **SportsStore.PaymentService**
- Consumes inventory success events
- Simulates payment processing
- Publishes:
  - `PaymentApproved`
  - `PaymentRejected`

#### **SportsStore.ShippingService**
- Consumes payment approval events
- Creates shipment information
- Publishes:
  - `ShippingCreated`

#### **SportsStore.Shared**
- Shared DTOs
- Event contracts
- Enums and common models

#### **SportsStore.CustomerPortal**
- Customer-facing application
- Used to browse products and place orders

#### **sportsstore.admindashboard.client**
- React admin dashboard
- Used to monitor and manage orders

---

## 🔄 Event Flow

The system processes orders asynchronously through RabbitMQ.


Customer Checkout
        ↓
Order API
        ↓
RabbitMQ Event Published (OrderSubmitted)
        ↓
Inventory Service
        ↓
InventoryConfirmed / InventoryFailed
        ↓
Payment Service
        ↓
PaymentApproved / PaymentRejected
        ↓
Shipping Service
        ↓
ShippingCreated
        ↓
Order API updates final status

Step-by-step Flow
Customer submits a checkout request
Order API creates the order
Order API publishes OrderSubmitted
Inventory Service validates stock
Payment Service processes payment
Shipping Service creates shipment
Order API updates the order status to Completed or Failed
📊 Order State Model

Orders move through a sequence of states during processing.

Example lifecycle
Submitted
→ InventoryPending
→ InventoryConfirmed
→ PaymentPending
→ PaymentApproved
→ ShippingPending
→ ShippingCreated
→ Completed

If a step fails, the order is marked as:

Failed
🌐 API Endpoints
Main endpoints implemented in Order API
POST /api/orders/checkout
GET /api/orders
GET /api/orders/{id}
GET /api/orders/{id}/status
GET /api/products
GET /api/customers/{id}/orders

These endpoints are used by both frontend applications and for testing through Swagger.

🖥️ Frontend Applications
1. Customer Portal (Blazor / Razor Pages)

The Customer Portal represents the customer-facing interface.

Features
View products
Add products to cart
Checkout
View previous orders
Track order status
2. Admin Dashboard (React)

The Admin Dashboard represents the operational/admin interface.

Features
View all orders
Display order status
View order details
Monitor order processing flow
Identify failed orders
Dashboard Information
Order ID
Customer Email
Status
Total amount
Order items
Design Decision

An inline details panel was used instead of more complex routing to keep the UI simple and stable.

⚙️ Logging with Serilog

Structured logging was implemented in the Order API using Serilog.

Logged actions include:
Order submission
Event publishing
Event consumption
Inventory validation
Payment outcome
Shipping creation
Errors and exceptions
Example log output
Checkout started for customer admin@test.com
Order created with total 119.98
OrderSubmitted event published
Order status updated to InventoryPending
Current logging output
Console logging enabled
Seq integration optional for future improvement
🔁 Object Mapping with AutoMapper

AutoMapper is used to simplify object mapping between:

DTOs → Entities
Entities → DTOs
API responses
Message contracts

This reduces repetitive manual mapping code and improves maintainability.

🗄️ Database

The system uses SQLite for persistence.

Main entities include:
Products
Orders
OrderItems

The Order API uses Entity Framework Core to manage data access and database operations.

🐳 Docker Setup

Docker Compose is used to run the distributed system in containers.

Services currently running with Docker
RabbitMQ
SportsStore.OrderApi
Why this setup was chosen

The goal was to prioritize stability and ensure the core order processing workflow runs reliably in Docker without major architectural changes.

▶️ How to Run
1. Run with Docker Compose
docker compose up --build
Access URLs
Order API Swagger: http://localhost:7040/swagger
RabbitMQ Management UI: http://localhost:15672
RabbitMQ Login
Username: sportsstore
Password: sports123
2. Run Frontend (Admin Dashboard)
npm install
npm run dev

Open in browser:

http://localhost:5173
✅ What is Working
Order API running successfully
RabbitMQ container running successfully
Docker Compose setup working
Order API connected to RabbitMQ
Swagger available for API testing
Event queues created in RabbitMQ
Order workflow functioning locally
Admin Dashboard working
Customer Portal included
🧪 Testing

Testing was mainly performed through:

Swagger API calls
RabbitMQ management dashboard
Manual validation of event flow
Order status monitoring through the admin interface
Verified scenarios
Checkout request creates an order
Events are published to RabbitMQ
Queues are created correctly
Order processing flow updates statuses
📌 Design Decisions

Several design decisions were made to prioritize successful delivery and system stability:

Keep the existing architecture working rather than introducing large refactors
Use RabbitMQ for asynchronous communication between services
Use SQLite for simplicity and easy setup
Dockerize the core services first (OrderApi + RabbitMQ)
Focus on integration and workflow rather than UI styling
Keep frontend functionality simple and stable

🚧 Limitations

Not all services are fully dockerized yet
CQRS is considered conceptually, but not fully implemented with MediatR
No authentication/authorization
Testing is mainly manual
Error recovery and retry handling can still be improved

🚀 Future Improvements

Dockerize all backend services
Add automated tests
Implement full CQRS with MediatR
Improve UI styling
Add authentication and authorization
Improve observability with Seq or centralized logging
Add retry logic and fault tolerance for messaging workflows

✅ Conclusion

SportsStore demonstrates a realistic distributed order processing platform using .NET microservices, RabbitMQ, and event-driven communication.

The system shows how asynchronous workflows can be used to process orders across multiple independent services while keeping components decoupled and maintainable.

This project reflects modern backend architecture practices used in scalable real-world applications.
