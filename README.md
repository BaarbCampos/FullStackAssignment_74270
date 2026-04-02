# SportsStore – Distributed Order Processing Platform

## 📌 Project Overview
SportsStore is a distributed e-commerce order processing system built using .NET microservices and RabbitMQ for asynchronous communication.

The platform simulates the full lifecycle of a customer order, from checkout to completion, using an event-driven architecture. Instead of processing everything inside a single application, the workflow is divided into multiple independent services that communicate through messages.

This project demonstrates how modern distributed systems handle order processing using:

- Event-driven architecture
- Decoupled services
- Asynchronous communication
- Microservice-based design

---

## 🧩 Technologies Used

### 🔹 Backend
- .NET / ASP.NET Core Web API
- C#
- RabbitMQ
- Entity Framework Core
- SQLite
- Serilog
- AutoMapper

### 🔹 Frontend
- React (Vite) – Admin Dashboard
- Blazor / Razor Pages – Customer Portal

### 🔹 DevOps / Deployment
- Docker
- Docker Compose

---

## 🏗️ System Architecture
The solution is organized into multiple services, each responsible for a specific step in the order processing workflow.

---

## 📦 Solution Structure

### SportsStore.OrderApi
- Central entry point of the system
- Exposes REST API endpoints
- Creates and stores orders
- Publishes events to RabbitMQ
- Tracks order status updates

### SportsStore.InventoryService
- Consumes order events
- Checks stock availability
- Publishes:
  - InventoryConfirmed
  - InventoryFailed

### SportsStore.PaymentService
- Consumes inventory success events
- Simulates payment processing
- Publishes:
  - PaymentApproved
  - PaymentRejected

### SportsStore.ShippingService
- Consumes payment approval events
- Creates shipment information
- Publishes:
  - ShippingCreated

### SportsStore.Shared
- Shared DTOs
- Event contracts
- Enums and common models

### SportsStore.CustomerPortal
- Customer-facing application
- Used to browse products and place orders

### sportsstore.admindashboard.client
- React admin dashboard
- Used to monitor and manage orders

---

## 🔄 Event Flow

The system processes orders asynchronously through RabbitMQ.

### Workflow

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

---

### Step-by-step Flow

1. Customer submits a checkout request  
2. Order API creates the order  
3. Order API publishes OrderSubmitted  
4. Inventory Service validates stock  
5. Payment Service processes payment  
6. Shipping Service creates shipment  
7. Order API updates the order status to Completed or Failed  

---

## 📊 Order State Model

Orders move through a sequence of states during processing.

### Example lifecycle

Submitted → InventoryPending → InventoryConfirmed → PaymentPending → PaymentApproved → ShippingPending → ShippingCreated → Completed  

If a step fails, the order is marked as:

Failed  

---

## 🌐 API Endpoints

Main endpoints implemented in Order API:

- POST /api/orders/checkout  
- GET /api/orders  
- GET /api/orders/{id}  
- GET /api/orders/{id}/status  
- GET /api/products  
- GET /api/customers/{id}/orders  

These endpoints are used by both frontend applications and for testing through Swagger.

---

## 🖥️ Frontend Applications

### 1. Customer Portal (Blazor / Razor Pages)

Features:
- View products  
- Add products to cart  
- Checkout  
- View previous orders  
- Track order status  

---

### 2. Admin Dashboard (React)

Features:
- View all orders  
- Display order status  
- View order details  
- Monitor order processing flow  
- Identify failed orders  

Dashboard Information:
- Order ID  
- Customer Email  
- Status  
- Total amount  
- Order items  

Design Decision:
An inline details panel was used instead of complex routing to keep the UI simple and stable.

---

## ⚙️ Logging with Serilog

Structured logging was implemented in the Order API using Serilog.

Logged actions include:
- Order submission  
- Event publishing  
- Event consumption  
- Inventory validation  
- Payment outcome  
- Shipping creation  
- Errors and exceptions  

Example log output:
- Checkout started for customer admin@test.com  
- Order created with total 119.98  
- OrderSubmitted event published  
- Order status updated to InventoryPending  

Current logging output:
- Console logging enabled  
- Seq integration optional  

---

## 🔁 Object Mapping with AutoMapper

AutoMapper is used to simplify object mapping between:

- DTOs → Entities  
- Entities → DTOs  
- API responses  
- Message contracts  

This reduces repetitive manual mapping and improves maintainability.

---

## 🗄️ Database

The system uses SQLite.

Main entities:
- Products  
- Orders  
- OrderItems  

Entity Framework Core is used for data access.

---

## 🐳 Docker Setup

Docker Compose is used to run the system.

Services:
- RabbitMQ  
- SportsStore.OrderApi  

---

## ▶️ How to Run

### Run with Docker

docker compose up --build

## 🐳 Docker Setup

Docker Compose is used to run the system.

Services:
- RabbitMQ  
- SportsStore.OrderApi  

---

## ▶️ How to Run

### Run with Docker

docker compose up --build
Access URLs
Order API Swagger: http://localhost:7040/swagger
RabbitMQ UI: http://localhost:15672
RabbitMQ Login
Username: sportsstore
Password: sports123
Run Frontend
npm install
npm run dev
Open in browser

http://localhost:5173

✅ What is Working

Order API running
RabbitMQ running
Docker Compose working
API connected to RabbitMQ
Swagger working
Event queues created
Order workflow functioning
Admin Dashboard working
Customer Portal working

🧪 Testing

Testing methods:

Swagger
RabbitMQ dashboard
Manual validation

Verified:

Orders are created
Events are published
Queues are working
Status updates correctly

📌 Design Decisions
Keep architecture stable
Use RabbitMQ for async communication
Use SQLite for simplicity
Dockerize core services first
Focus on workflow over UI
Keep frontend simple

🚧 Limitations

Not all services dockerized
CQRS not fully implemented
No authentication
Manual testing only
Retry logic missing

🚀 Future Improvements

Dockerize all services
Add automated tests
Implement CQRS with MediatR
Improve UI
Add authentication
Add centralized logging (Seq)
Improve fault tolerance

✅ Conclusion

SportsStore demonstrates a distributed order processing system using .NET microservices and RabbitMQ.

It shows how asynchronous workflows enable scalable and decoupled systems.

This project reflects real-world backend architecture practices.
