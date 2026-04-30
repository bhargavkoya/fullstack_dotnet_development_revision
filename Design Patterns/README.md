# Design Patterns Solution

This folder contains the full order-management portfolio solution:

- `OrderApi/` - ASP.NET Core 8 Web API with JWT auth, OAuth support, authorization policies, design patterns, and in-memory persistence.
- `OrderApi.BlazorClient/` - Blazor WebAssembly frontend built with MudBlazor, local token storage, typed HTTP clients, and the full customer/admin order flow.
- `OrderApi.Tests/` - xUnit test project for API behavior and supporting infrastructure.
- `OrderManagement.slnx` - solution entry point for the `Design Patterns` folder.

## What The Solution Demonstrates

The repository is structured as a portfolio/resume project that shows both backend and frontend engineering practices:

- SOLID principles across services, handlers, clients, and state management.
- Design patterns such as Proxy, Decorator, Observer, Strategy, Builder, Factory, Template Method, Chain of Responsibility, CQRS, and more.
- JWT login, refresh-token handling, OAuth entry/callback flow, and role-based authorization.
- A clean UI shell with MudBlazor on the Blazor client.
- API-driven order management with create, browse, detail, and cancel workflows.

## Local Run Guide

### Prerequisites

- .NET 8 SDK installed
- Two terminals available
- HTTPS development certificates configured if your local environment requires them

### Step 1: Open the solution folder

Open the `Design Patterns` folder in your editor or terminal.

### Step 2: Run the API

In the first terminal:

```bash
cd "Design Patterns/OrderApi"
dotnet restore
dotnet build
dotnet run
```

The API should start on `https://localhost:5001`.

Useful API endpoint while testing:

- Swagger: `https://localhost:5001/swagger`

### Step 3: Verify API CORS for the Blazor client

The API must allow requests from the Blazor client origin:

- `https://localhost:5000`

If you need to confirm it manually, the API should register a CORS policy for that origin before authentication middleware.

### Step 4: Run the Blazor client

In a second terminal:

```bash
cd "Design Patterns/OrderApi.BlazorClient"
dotnet restore
dotnet build
dotnet run
```

The client should open on `https://localhost:5000`.

### Step 5: Sign in and test the UI flow

Use the seeded credentials shown on the login screen:

- Admin: `admin@orderapi.com` / `Admin123!`
- Customer: `customer@orderapi.com` / `Customer123!`

Then verify the main flow:

1. Login
2. View orders
3. Place an order
4. Open order details and cancel if permitted
5. Logout

### Step 6: Run tests optionally

From the repository root or the `Design Patterns` folder:

```bash
dotnet test "Design Patterns/OrderApi.Tests/OrderApi.Tests.csproj"
```

## Project Notes

### OrderApi

- REST API for order management.
- JWT authentication plus OAuth entry points.
- In-memory storage for easy local demo use.
- Includes endpoints for login, refresh, logout, OAuth, order browse, order detail, and cancel.

### OrderApi.BlazorClient

- Standalone Blazor WebAssembly app.
- Uses MudBlazor for the full UI shell and pages.
- Persists tokens in browser local storage.
- Calls the API through typed HTTP clients and a token-injecting handler.
- Supports login, OAuth callback handling, order browsing, order placement, order detail, and cancellation.

### OrderApi.Tests

- Covers API behavior with automated tests.
- Useful for validating changes to auth, orders, repositories, and supporting infrastructure.

## Troubleshooting

- If the client cannot authenticate, make sure the API is running first on `https://localhost:5001`.
- If the browser blocks requests, verify the API CORS policy allows `https://localhost:5000`.
- If HTTPS warnings appear on first run, trust the local development certificate for your machine.
- If OAuth is unavailable in your environment, use the seeded local credentials instead.

## Repository Layout

- `Design Patterns/OrderApi/` - API project
- `Design Patterns/OrderApi.BlazorClient/` - Blazor frontend
- `Design Patterns/OrderApi.Tests/` - tests
- `Design Patterns/OrderManagement.slnx` - solution file

## Summary

Run the API first, then the Blazor client, and use the seeded credentials to verify the full end-to-end order-management experience locally.
