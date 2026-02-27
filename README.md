# SportsStore – .NET Upgrade & Stripe Integration

## Overview

This project upgrades the original SportsStore application to .NET 9 and integrates Stripe payments with proper architecture, logging, and CI.

---

## .NET Upgrade

- Upgraded to .NET 9
- EF Core updated
- Project builds successfully
- Database migration applied

---

## Stripe Integration

- Implemented using service abstraction:
  - `IStripePaymentService`
  - `StripePaymentService`
- Controllers depend only on the interface
- Orders are saved **only after payment confirmation**
- Stripe session data stored in database

---

## Logging (Serilog)

- Structured logging enabled
- Logs written to console and rolling files
- Logs include checkout attempts, payment status, and order creation

---

## Testing

- Checkout flow updated to async
- Stripe service mocked in unit tests
- All tests passing
- CI pipeline configured with GitHub Actions

---

## CI

GitHub Actions pipeline runs:
- Restore
- Build
- Test

Workflow status: Passing

---

## Author

Barbara Campos 74270
