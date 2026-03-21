# SportsStore - Assignment 2

## Project Overview

SportsStore is a distributed e-commerce order processing system built using .NET microservices and RabbitMQ for asynchronous communication.

The project simulates the lifecycle of a customer order from checkout to completion.  
Instead of processing everything inside a single application, the system uses separate services that communicate through events.

The current implementation includes:

- Order API
- Inventory Service
- Payment Service
- Shipping Service
- Shared contracts library
- RabbitMQ integration
- Asynchronous event-driven workflow

The main goal of this project is to demonstrate a microservices architecture with decoupled services, message-based communication, and status tracking throughout the order lifecycle.

---

## Architecture

The solution is organized into multiple projects, each with a specific responsibility.

### Solution Structure

- `SportsStore.OrderApi`  
  Handles order creation, exposes API endpoints, publishes the initial event, and updates the final order status.

- `SportsStore.InventoryService`  
  Consumes order submission events and publishes inventory confirmation events.

- `SportsStore.PaymentService`  
  Consumes inventory confirmation events and publishes payment approval events.

- `SportsStore.ShippingService`  
  Consumes payment approval events and publishes shipping creation events.

- `SportsStore.Shared`  
  Contains shared DTOs, enums, and event contracts used across the services.

- `SportsStore`  
  Legacy/original project from the previous assignment phase.

- `SportsStore.Tests`  
  Placeholder for tests and future validation.

### Architectural Style

This project follows an **event-driven microservices architecture**.

Each service is isolated and communicates asynchronously through RabbitMQ queues.  
This improves separation of concerns and simulates how real distributed systems handle workflows across multiple components.

---

## Services

### 1. SportsStore.OrderApi
Responsibilities:
- Exposes REST endpoints for order operations
- Accepts checkout requests
- Creates orders
- Publishes the `OrderSubmitted` event
- Consumes the final `ShippingCreated` event
- Updates the order status to `Completed`

Main endpoints:
- `POST /api/orders/checkout`
- `GET /api/orders/{id}`

---

### 2. SportsStore.InventoryService
Responsibilities:
- Consumes `OrderSubmitted`
- Simulates inventory validation
- Publishes `InventoryConfirmed`

---

### 3. SportsStore.PaymentService
Responsibilities:
- Consumes `InventoryConfirmed`
- Simulates payment processing
- Publishes `PaymentApproved`

---

### 4. SportsStore.ShippingService
Responsibilities:
- Consumes `PaymentApproved`
- Simulates shipment creation
- Publishes `ShippingCreated`

---

### 5. SportsStore.Shared
Responsibilities:
- Shared event contracts
- Shared DTOs
- Shared order status enum

Examples:
- `OrderSubmitted`
- `InventoryConfirmed`
- `PaymentApproved`
- `ShippingCreated`
- `OrderStatus`

---

## Event Flow

The system currently supports the successful order workflow below:

1. Customer sends a checkout request to the Order API
2. Order API creates an order with status `Submitted`
3. Order API publishes `OrderSubmitted`
4. Inventory Service consumes `OrderSubmitted`
5. Inventory Service publishes `InventoryConfirmed`
6. Payment Service consumes `InventoryConfirmed`
7. Payment Service publishes `PaymentApproved`
8. Shipping Service consumes `PaymentApproved`
9. Shipping Service publishes `ShippingCreated`
10. Order API consumes `ShippingCreated`
11. Order API updates the order status to `Completed`

### Event Pipeline

```text
OrderApi
 -> order-submitted

InventoryService
 -> inventory-confirmed

PaymentService
 -> payment-approved

ShippingService
 -> shipping-created

OrderApi
 -> order status updated to Completed
