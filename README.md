# SportsStore Full Stack Assignment



## Overview

This project is an upgraded and extended version of the SportsStore application originally based on the Pro ASP.NET Core 6 project.

The assignment work completed includes:

- Upgrade from .NET 6 to .NET 8
- Structured logging with Serilog
- Logging to console, rolling file, and Seq
- Stripe payment integration using test mode
- Successful and failed payment handling
- Storage of payment information with the order
- GitHub-based source control workflow
- Automated testable solution structure with unit tests

---

## Technology Stack

- ASP.NET Core MVC / Razor Pages
- .NET 8
- Entity Framework Core 8
- SQL Server LocalDB
- Serilog
- Seq
- Stripe.NET
- xUnit
- Moq

---

## Upgrade Steps

The original SportsStore solution targeted .NET 6.  
The following upgrade work was completed:

- Updated `SportsStore` project from `net6.0` to `net8.0`
- Updated `SportsStore.Tests` project from `net6.0` to `net8.0`
- Updated Entity Framework Core packages to version 8
- Updated test package versions for compatibility with .NET 8
- Verified that the application builds and runs correctly after upgrade

---

## Logging Setup

Serilog was added to provide structured logging across the application lifecycle.

### Logging destinations

- Console
- Rolling log files
- Seq

### Structured logging added for

- Application startup
- Request logging
- Cart activity
- Checkout activity
- Payment processing
- Payment success
- Payment failure
- Order creation
- Exceptions

### Example structured properties logged

- `ProductId`
- `ProductName`
- `CartItemCount`
- `CartTotal`
- `CustomerName`
- `OrderId`
- `PaymentIntentId`
- `PaymentStatus`
- `Currency`

### Seq

Seq was run locally using Docker and configured through `appsettings.json`.

Example Docker command used:

```bash
docker run --name seq -d -p 5341:80 datalust/seq