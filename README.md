🚀 SportsStore - Assignment 2
📌 Project Overview

SportsStore is a distributed e-commerce order processing system built using .NET microservices and RabbitMQ for asynchronous communication, combined with a React Admin Dashboard for order management.

The system simulates the full lifecycle of a customer order, from checkout to completion. Instead of processing everything in a single application, the system is divided into multiple independent services that communicate through events.

This project demonstrates how modern distributed systems handle workflows using event-driven architecture, decoupled services, and message-based communication.

🧩 Technologies Used
Backend
.NET (ASP.NET Core Web API)
RabbitMQ
C#
Microservices Architecture
Frontend
React (Vite)
JavaScript
CSS (custom styling)
Other Concepts
Event-driven architecture
Asynchronous communication
Distributed systems
CQRS (conceptually)
Separation of concerns
🏗️ Architecture

The system is structured into multiple services, each responsible for a specific part of the order lifecycle.

📦 Solution Structure
Project	Description
SportsStore.OrderApi	Entry point for orders (REST API)
SportsStore.InventoryService	Handles stock validation
SportsStore.PaymentService	Handles payment processing
SportsStore.ShippingService	Handles shipment creation
SportsStore.Shared	Shared DTOs, events, and enums
SportsStore	Legacy project from Assignment 1
SportsStore.Tests	Testing placeholder
⚙️ Architectural Style

This project follows an event-driven microservices architecture.

Each service:

runs independently
communicates via RabbitMQ
reacts to events instead of direct calls
Benefits:
loose coupling
scalability
better separation of concerns
realistic distributed system simulation
🔄 Event Flow (Order Lifecycle)

The system supports a full successful order workflow:

Customer sends a checkout request → Order API
Order is created with status Submitted
OrderSubmitted event is published

Then the pipeline continues:

OrderApi
 → OrderSubmitted

InventoryService
 → InventoryConfirmed

PaymentService
 → PaymentApproved

ShippingService
 → ShippingCreated

OrderApi
 → Order status updated to Completed
Step-by-step:
Inventory Service validates stock
Payment Service processes payment
Shipping Service creates shipment
Order API updates final status to Completed
🔧 Services Description
🟦 Order API

Responsibilities:

Exposes REST endpoints
Creates orders
Publishes initial event (OrderSubmitted)
Receives final event (ShippingCreated)
Updates order status

Endpoints:

POST /api/orders/checkout
GET /api/orders/{id}
🟨 Inventory Service
Consumes: OrderSubmitted
Publishes: InventoryConfirmed
Simulates stock validation
🟩 Payment Service
Consumes: InventoryConfirmed
Publishes: PaymentApproved
Simulates payment processing
🟥 Shipping Service
Consumes: PaymentApproved
Publishes: ShippingCreated
Simulates shipment creation
🧱 Shared Project

Contains shared components across services:

Event contracts:
OrderSubmitted
InventoryConfirmed
PaymentApproved
ShippingCreated
DTOs
OrderStatus enum
🖥️ Frontend (Admin Dashboard)

The project includes a React Admin Dashboard for managing and visualizing orders.

Features:
View list of orders
Display:
Order ID
Email
Status (with color indicators)
Total amount
Filter orders by status
View order details inline (no routing required)
Display order items (products, quantity, price)
Design Decisions:
Inline details panel instead of routing
more stable
avoids breaking navigation
Focus on functionality first, UI later
📊 Example UI Features
Status badges:
🟢 Completed
🔴 Failed
🟡 Pending
Summary cards:
total orders
completed
failed
Interactive filtering
Smooth user experience without page reload
▶️ How to Run the Project
1. Start RabbitMQ

Make sure RabbitMQ is running:

docker run -d -p 5672:5672 -p 15672:15672 rabbitmq:management

Access dashboard:

http://localhost:15672
guest / guest
2. Run Backend Services

Start each service:

OrderApi
InventoryService
PaymentService
ShippingService

Each should run in a separate terminal or instance.

3. Run Frontend

Navigate to frontend folder:

npm install
npm run dev

Open:

http://localhost:5173
📌 Key Concepts Demonstrated
Microservices architecture
Event-driven systems
Asynchronous messaging with RabbitMQ
Decoupled service communication
State transitions through events
Frontend integration with distributed backend
🚧 Future Improvements
Add failure handling (retry, dead-letter queues)
Implement full CQRS pattern
Add logging (Serilog)
Add AutoMapper
Dockerize all services
Improve UI with component library (e.g., Material UI)
Add authentication and authorization
✅ Conclusion

This project demonstrates how a distributed order-processing system can be built using microservices, RabbitMQ, and event-driven communication.

Each service plays a specific role in the workflow, resulting in a scalable and maintainable architecture that reflects real-world enterprise systems.
