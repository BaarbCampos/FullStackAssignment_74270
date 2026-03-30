# 🚀 SportsStore - Assignment 2

## 📌 Project Overview

SportsStore is a distributed e-commerce order processing system built using .NET microservices and RabbitMQ for asynchronous communication, combined with a React Admin Dashboard for order management.

The system simulates the full lifecycle of a customer order, from checkout to completion. Instead of processing everything in a single application, the system is divided into multiple independent services that communicate through events.

This project demonstrates how modern distributed systems handle workflows using event-driven architecture, decoupled services, and message-based communication.

---

## 🧩 Technologies Used

### 🔹 Backend
- .NET (ASP.NET Core Web API)
- C#
- RabbitMQ
- Microservices Architecture

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

- **SportsStore.OrderApi**  
  Handles order creation, exposes API endpoints, publishes events, and updates order status.

- **SportsStore.InventoryService**  
  Consumes order events and validates inventory.

- **SportsStore.PaymentService**  
  Processes payments.

- **SportsStore.ShippingService**  
  Creates shipment after payment.

- **SportsStore.Shared**  
  Shared DTOs, enums, and event contracts.

- **SportsStore**  
  Legacy project from Assignment 1.

- **SportsStore.Tests**  
  Placeholder for future testing.

---

## 🔄 Event Flow

The system follows this order processing pipeline:

```text
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
Filter orders by status
View order details inline
Display order items (products, quantity, price)
Design Decisions:
Used inline details panel instead of routing for stability
Focused on functionality first, UI improvements later

## ▶️ How to Run

### 1. Run Backend Services

Run each service individually:

- OrderApi
- InventoryService
- PaymentService
- ShippingService

---

### 2. Run Frontend

```bash
npm install
npm run dev

Open in browser:

http://localhost:5173
🚧 Future Improvements
Add error handling (failures in services)
Implement logging (Serilog)
Add AutoMapper
Implement full CQRS pattern
Dockerize all services
Improve UI with component library
Add authentication
✅ Conclusion

This project demonstrates a distributed system using microservices and RabbitMQ. Each service is independent and communicates through events, creating a scalable and maintainable architecture similar to real-world systems.
