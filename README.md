# 🚀 SportsStore - Assignment 2

## 📌 Project Overview

SportsStore is a distributed e-commerce order processing system built using **.NET microservices** and **RabbitMQ** for asynchronous communication, combined with a **React Admin Dashboard** for order management.

The system simulates the full lifecycle of a customer order, from checkout to completion. Instead of processing everything in a single application, the system is divided into multiple independent services that communicate through events.

This project demonstrates how modern distributed systems handle workflows using:

- Event-driven architecture  
- Decoupled services  
- Message-based communication  

---

## 🧩 Technologies Used

### 🔹 Backend
- .NET (ASP.NET Core Web API)  
- C#  
- RabbitMQ  
- Entity Framework Core (SQLite)  
- Serilog (logging)  
- AutoMapper  

### 🔹 Frontend
- React (Vite)  
- JavaScript  
- CSS (Custom Styling)  

### 🔹 Concepts
- Event-driven architecture  
- Asynchronous communication  
- Distributed systems  
- CQRS (conceptual)  
- Separation of concerns  

---

## 🏗️ Architecture

The solution is organized into multiple services, each responsible for a specific part of the order lifecycle.

### 📦 Solution Structure

**SportsStore.OrderApi**
- Handles order creation  
- Exposes REST endpoints  
- Publishes events (OrderSubmitted)  
- Updates final order status  

**SportsStore.InventoryService**
- Consumes order events  
- Validates inventory  
- Publishes InventoryConfirmed  

**SportsStore.PaymentService**
- Processes payments  
- Publishes PaymentApproved  

**SportsStore.ShippingService**
- Creates shipment  
- Publishes ShippingCreated  

**SportsStore.Shared**
- Shared DTOs  
- Enums  
- Event contracts  

**SportsStore**
- Legacy project from Assignment 1  

**SportsStore.Tests**
- Placeholder for future testing  

---

## 🔄 Event Flow


OrderApi → order-submitted
InventoryService → inventory-confirmed
PaymentService → payment-approved
ShippingService → shipping-created
OrderApi → order completed


Step-by-step flow:
Customer sends checkout request
Order is created (Submitted)
Inventory is validated
Payment is processed
Shipping is created
Order is marked as Completed
🖥️ Frontend (Admin Dashboard)

The project includes a React Admin Dashboard for managing orders.

Features:
View list of orders
Display:
Order ID
Email
Status (with color indicators)
Total amount
View order details inline
Display order items:
Product
Quantity
Price
Design Decisions:
Used inline details panel instead of routing (simpler and more stable)
Focused on functionality first, UI improvements later
⚙️ Logging (Serilog)

Basic logging was implemented in the Order API using Serilog.

Logs include:

Incoming HTTP requests
Order creation
Database operations
Event publishing
Status updates

Example:

Checkout started for customer admin@test.com
Order created with total 119.98
OrderSubmitted event published
Order status updated to InventoryPending

Logs are currently written to:

Console (working)
Seq (optional / can be configured)
🔁 Object Mapping (AutoMapper)

AutoMapper was added to simplify mapping between:

DTOs → Entities
Entities → DTOs

This reduces manual mapping code and improves maintainability.

▶️ How to Run
1. Run Backend Services

Run each service individually:

OrderApi
InventoryService
PaymentService
ShippingService
2. Run Frontend
npm install
npm run dev

Open in browser:

http://localhost:5173

🚧 Future Improvements
Add error handling (service failures)
Improve logging (Seq integration)
Implement full CQRS pattern
Dockerize all services
Improve UI with component library
Add authentication

✅ Conclusion

This project demonstrates a distributed system using microservices and RabbitMQ.

Each service is independent and communicates through events, creating a scalable, maintainable, and realistic architecture similar to modern production systems.
