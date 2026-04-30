# OrderApi.BlazorClient

Blazor WebAssembly frontend for the Order Management API.

## Stack

- .NET 8 Blazor WebAssembly, standalone
- MudBlazor UI
- Blazored.LocalStorage for token persistence
- JWT-based auth with OAuth callback handling
- Typed HTTP clients with `HttpClientFactory`

## Setup

1. Start the API first at `https://localhost:5001`.
2. Make sure the API allows CORS for the Blazor origin `https://localhost:5000`.
3. Start this client with `dotnet run` from the `OrderApi.BlazorClient` project.
4. Open the app at `https://localhost:5000`.

## Client Configuration

The client reads API settings from `wwwroot/appsettings.json`.

- `ApiBaseUrl`: API root URL
- `AppSettings.AppName`: display title
- `AppSettings.OAuthCallbackPath`: OAuth return route

## Pattern Index

| Pattern | File | What it demonstrates |
|---|---|---|
| Proxy | `Auth/JwtAuthStateProvider.cs` | JWT-backed auth state with silent refresh and claim parsing |
| Decorator | `HttpClients/AuthenticatedHttpHandler.cs` | Adds bearer tokens and retries once after refresh on 401 |
| Decorator | `Services/NotificationService.cs` | Wraps snackbar usage with consistent messaging |
| Observer | `State/AppState.cs` | Observable order state that notifies subscribed components |
| Strategy | `State/AppStateReducer.cs` | Pure state transition functions for order collections |
| Builder | `Builders/PlaceOrderRequestBuilder.cs` | Fluent construction and validation of place-order payloads |
| Factory | `Factories/OrderRequestBuilderFactory.cs` | Pre-configures builders with sensible defaults |
| Template Method | `HttpClients/ApiClientBase.cs` | Shared HTTP execution pipeline for all API clients |
| CQRS | `Services/OrderService.cs` | Separates query and command flows on the client |
| Chain of Responsibility | `App.razor`, `Shared/RedirectToLogin.razor` | Router-level auth short-circuiting and role-aware redirects |
| SRP | `Auth/TokenManager.cs` | LocalStorage token persistence only |
| SRP | `Services/AuthService.cs` | Auth orchestration only |
| SRP | `Services/OrderService.cs` | Order orchestration only |
| SRP | `Components/Orders/PlaceOrderForm.razor` | Order item entry UI only |
| ISP | `HttpClients/IAuthApiClient.cs` | Auth HTTP contract only |
| ISP | `HttpClients/IOrderApiClient.cs` | Order HTTP contract only |
| DIP | `Program.cs` | Wires abstractions to concrete implementations |
| LSP | `HttpClients/AuthApiClient.cs`, `HttpClients/OrderApiClient.cs` | Both clients remain substitutable through `ApiClientBase` |

## Auth Flow

```text
[Login page]
     |
     v
[AuthService.LoginAsync]
     |
     v
[AuthApiClient POST /api/auth/login]
     |
     v
[TokenManager saves access + refresh tokens]
     |
     v
[JwtAuthStateProvider marks user authenticated]
     |
     v
[App navigates to /orders/my]

OAuth flow:
[Login page] -> [AuthService.GetOAuthUrlAsync]
             -> [API authorization URL]
             -> [Provider redirect]
             -> [API callback issues token]
             -> [Blazor /oauth-callback]
             -> [TokenManager + auth state update]
             -> [/orders/my]

Silent refresh:
[401 response] -> [AuthenticatedHttpHandler]
               -> [AuthService.RefreshAsync]
               -> [TokenManager update]
               -> [retry original request once]
```

## Pages

### Login

- Centered MudCard login shell
- Local login with seeded credentials helper text
- Google and GitHub OAuth launch buttons
- Error alert for auth failures

### My Orders

- Authenticated customer view
- Loads orders into `AppState`
- Shows order cards in a responsive grid
- Empty-state message when no orders exist

### All Orders

- Admin view with MudDataGrid
- Search and status filtering
- Cancel action with confirmation dialog
- Role-aware unauthorized view

### Place Order

- Two-step MudStepper order flow
- Item entry step with live totals
- Payment step with provider selection and summary
- Builder + factory used for the request payload

### Order Detail

- Individual order view
- Item table
- Cancel order action when permitted
- Unauthorized screen when the API returns 403

### OAuth Callback

- Reads `token` and `refresh` query values
- Persists tokens and updates auth state
- Redirects to the order list on success

## Screenshot Notes

- Login page: centered app card, seeded credentials, and OAuth buttons.
- My Orders page: responsive order card grid with empty-state alert.
- All Orders page: admin data grid with search, status filter, and cancel actions.
- Place Order page: two-step form with item editing and payment selection.
- Order Detail page: summary card, item table, and conditional cancel button.
- Unauthorized page: compact permission-denied screen.
- OAuth callback page: spinner-only transient loading view.

## API Notes

The API must allow CORS from `https://localhost:5000` before authentication middleware runs.

Example API policy:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy.WithOrigins("https://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

app.UseCors("BlazorClient");
```

## Validation

The client project builds successfully with `dotnet build`.
